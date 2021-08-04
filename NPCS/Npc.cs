using Exiled.API.Extensions;
using Exiled.API.Features;
using FakePlayers.API;
using MEC;
using Microsoft.Scripting.Hosting;
using NPCS.AI.Python;
using NPCS.Navigation;
using NPCS.Talking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NPCS
{
    public class Npc : FakePlayer
    {
        #region Serialization

        //Megabruh
        public class NPCMappingInfo
        {
            private Npc parent;

            private string deserializedRoom;
            private float deserializedRoomRotation;
            private Utils.SerializableVector3 deserializedRelative;
            private Utils.SerializableVector2 deserializedRotations;
            private string deserializedFile;

            public NPCMappingInfo(Npc which)
            {
                parent = which;
            }

            public NPCMappingInfo()
            {
            }

            public string Room
            {
                get
                {
                    return parent?.PlayerInstance.CurrentRoom.Name ?? deserializedRoom;
                }
                set
                {
                    deserializedRoom = value;
                }
            }

            public float RoomRotation
            {
                get
                {
                    return parent?.PlayerInstance.CurrentRoom.Transform.localRotation.eulerAngles.y ?? deserializedRoomRotation;
                }
                set
                {
                    deserializedRoomRotation = value;
                }
            }

            public Utils.SerializableVector3 Relative
            {
                get
                {
                    Vector3? source = parent?.PlayerInstance.Position - parent?.PlayerInstance.CurrentRoom.Position;
                    if (source == null)
                    {
                        return deserializedRelative;
                    }
                    else
                    {
                        return new Utils.SerializableVector3(source.Value);
                    }
                }
                set
                {
                    deserializedRelative = value;
                }
            }

            public Utils.SerializableVector2 Rotation
            {
                get
                {
                    Vector2? source = parent?.PlayerInstance.Rotations;
                    if (source == null)
                    {
                        return deserializedRotations;
                    }
                    else
                    {
                        return new Utils.SerializableVector2(source.Value);
                    }
                }
                set
                {
                    deserializedRotations = value;
                }
            }

            public string File
            {
                get
                {
                    return parent?.SaveFile ?? deserializedFile;
                }
                set
                {
                    deserializedFile = value;
                }
            }
        }

        #endregion Serialization

        #region Properties

        public enum MovementDirection
        {
            NONE,
            FORWARD,
            BACKWARD,
            LEFT,
            RIGHT
        };

        public enum TargetLostBehaviour
        {
            STOP,
            TELEPORT,
            SEARCH,
        }

        public TalkNode RootNode { get; set; }

        public string SaveFile { get; set; } = null;

        public Dictionary<Player, TalkNode> TalkingStates { get; } = new Dictionary<Player, TalkNode>();

        public MovementDirection CurMovementDirection { get; set; }

        public bool IsLocked { get; set; } = false;

        public Player LockHandler { get; set; } = null;

        public bool IsActionLocked { get; set; } = false;

        public bool IsExclusive { get; set; } = false;

        public LinkedList<NavigationNode> NavigationQueue { get; private set; } = new LinkedList<NavigationNode>();

        public NavigationNode CurrentNavTarget { get; set; } = null;

        public Player FollowTarget { get; set; } = null;

        public TargetLostBehaviour OnTargetLostBehaviour { get; set; } = TargetLostBehaviour.TELEPORT;

        private float __customSpeed = 0f;

        private static int __counter = 0;

        public float MovementSpeed
        {
            get
            {
                if (__customSpeed > 0)
                {
                    return __customSpeed;
                }
                else
                {
                    return IsRunning ? CharacterClassManager._staticClasses[(int)PlayerInstance.Role].runSpeed : CharacterClassManager._staticClasses[(int)PlayerInstance.Role].walkSpeed;
                }
            }
            set
            {
                __customSpeed = value;
            }
        }

        public bool DisableRun { get; set; } = false;
        public bool IsRunning { get; set; } = false;

        public List<CoroutineHandle> MovementCoroutines { get; } = new List<CoroutineHandle>();

        //AI STATES -------------------------------
        public bool AIEnabled { get; set; } = false;

        public string AIScript { get; set; }

        // ============ Python
        public NPCAIController AIController { get; set; } = null;
        public ScriptScope ScriptScope { get; private set; } = null;
        public Player CurrentAIPlayerTarget { get; set; } = null;
        public Room CurrentAIRoomTarget { get; set; } = null;
        public string CurrentAIItemGroupTarget { get; set; } = null;
        public NavigationNode CurrentAIItemNodeTarget { get; set; } = null;
        public NPCAIHelper AIHelper { get; set; } = null;
        //------------------------------------------

        #endregion Properties

        #region Coroutines

        public void StartAI()
        {
            AttachedCoroutines.Add(Timing.RunCoroutine(AICoroutine()));
        }

        private IEnumerator<float> AICoroutine()
        {
            while (!IsValid)
            {
                yield return Timing.WaitForOneFrame;
            }
            Func<NPCAIController, NPCAIHelper, float, float> tick = null;
            if (!string.IsNullOrEmpty(AIScript))
            {
                try
                {
                    ScriptScope = Plugin.Engine.ExecuteFile(AIScript);
                    tick = ScriptScope.GetVariable<Func<NPCAIController, NPCAIHelper, float, float>>("Tick");
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to prepare AI script: {e}");
                }
            }
            for (; ; )
            {
                if (AIEnabled && tick != null)
                {
                    float delay = Plugin.Instance.Config.AIIdleUpdateFrequency;
                    try
                    {
                        delay = tick.Invoke(AIController, AIHelper, Plugin.Instance.Config.AIIdleUpdateFrequency);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"AI script failure: {e}");
                    }
                    if (delay < Plugin.Instance.Config.AIIdleUpdateFrequency)
                    {
                        delay = Plugin.Instance.Config.AIIdleUpdateFrequency;
                    }
                    yield return Timing.WaitForSeconds(delay);
                }
                else
                {
                    yield return Timing.WaitForSeconds(Plugin.Instance.Config.AIIdleUpdateFrequency);
                }
            }
        }

        private IEnumerator<float> NavCoroutine()
        {
            Queue<Vector3> FollowTargetPosCache = new Queue<Vector3>();
            int eta = 0;
            int dormant_cache_update = 0;
            for (; ; )
            {
                while (!IsValid)
                {
                    yield return 0.0f;
                }
                if (FollowTarget != null)
                {
                    if (FollowTarget.IsAlive)
                    {
                        float dist = Vector3.Distance(FollowTarget.Position, PlayerInstance.Position);

                        //If we are far away...
                        if (dist >= Plugin.Instance.Config.MaxFollowDistance)
                        {
                            if (OnTargetLostBehaviour == TargetLostBehaviour.TELEPORT)
                            {
                                //... Teleport to player if allowed
                                PlayerInstance.Position = FollowTarget.Position;
                                eta = 0;
                                FollowTargetPosCache.Clear();
                            }
                            else
                            {
                                //Stop or try to search otherwise

                                //FireEvent(new NPCTargetLostEvent(this, FollowTarget));

                                FollowTargetPosCache.Clear();
                                eta = 0;
                                Stop();
                                if (OnTargetLostBehaviour == TargetLostBehaviour.SEARCH)
                                {
                                    Room r = FollowTarget.CurrentRoom;
                                    if (r != null)
                                    {
                                        GotoRoom(r);
                                    }
                                }
                                continue;
                            }
                        }

                        //If target is not near
                        if (dist >= 1.5f)
                        {
                            //Update Pos cache each third tick
                            if (dormant_cache_update > 2)
                            {
                                FollowTargetPosCache.Enqueue(FollowTarget.Position);
                                dormant_cache_update = 0;
                            }
                            dormant_cache_update++;
                        }
                        else
                        {
                            //Otherwise just dont move
                            FollowTargetPosCache.Clear();
                            eta = 0;
                            Timing.KillCoroutines(MovementCoroutines.ToArray());
                            Move(MovementDirection.NONE);
                        }
                    }
                    else
                    {
                        // Target dead, reset
                        FollowTargetPosCache.Clear();
                        eta = 0;
                        //FireEvent(new NPCFollowTargetDiedEvent(this, FollowTarget));
                        Stop();
                        continue;
                    }

                    //If we reached predicted target
                    if (eta <= 0)
                    {
                        //Schedule next position
                        if (!FollowTargetPosCache.IsEmpty())
                        {
                            float full_eta = GoTo(FollowTargetPosCache.Dequeue());
                            eta = (int)(full_eta / Plugin.Instance.Config.NavUpdateFrequency) - 1;
                        }
                    }
                    else
                    {
                        eta--;
                    }
                }
                else
                {
                    //Noone to follow, try taking nodes from nav queue

                    eta = 0;
                    FollowTargetPosCache.Clear();

                    if (CurrentNavTarget != null)
                    {
                        IsRunning = !DisableRun;
                        //There is current
                        float distance = Vector3.Distance(CurrentNavTarget.Position, PlayerInstance.Position);

                        if (distance < 3f)
                        {
                            //Try to open the door if there is one, so we wont collide with it
                            if (CurrentNavTarget.AttachedDoor != null && !CurrentNavTarget.AttachedDoor.NetworkTargetState)
                            {
                                Inventory.SyncItemInfo prev = PlayerInstance.CurrentItem;
                                bool open = CurrentNavTarget.AttachedDoor.RequiredPermissions.CheckPermissions(prev.id, PlayerInstance.ReferenceHub);
                                if (!open)
                                {
                                    foreach (Inventory.SyncItemInfo keycard in PlayerInstance.Inventory.items.Where(i => i.id.IsKeycard()))
                                    {
                                        if (CurrentNavTarget.AttachedDoor.RequiredPermissions.CheckPermissions(keycard.id, PlayerInstance.ReferenceHub))
                                        {
                                            PlayerInstance.CurrentItem = keycard;
                                            open = true;
                                            break;
                                        }
                                    }
                                }
                                if (open)
                                {
                                    //All is good
                                    Timing.KillCoroutines(MovementCoroutines.ToArray());
                                    Move(MovementDirection.NONE);
                                    while (CurrentNavTarget.AttachedDoor.ActiveLocks > 0)
                                    {
                                        yield return 0f;
                                    }
                                    yield return Timing.WaitForSeconds(0.2f);
                                    CurrentNavTarget.AttachedDoor.NetworkTargetState = true;
                                    yield return Timing.WaitForSeconds(0.1f);
                                    GoTo(CurrentNavTarget.Position);
                                    PlayerInstance.CurrentItem = prev;
                                }
                                else
                                {
                                    //Stop otherwise
                                    Stop();
                                    continue;
                                }
                            }
                        }

                        if (distance < 6)
                        {
                            NavigationNode NextNavTarget = NavigationQueue.First?.Value;

                            NavigationNode lift_node = null;
                            if (NextNavTarget != null)
                            {
                                if (NextNavTarget.AttachedElevator != null)
                                {
                                    lift_node = NextNavTarget;
                                }
                            }
                            //If there is an elevator, try to use it
                            if (CurrentNavTarget.AttachedElevator == null && lift_node != null)
                            {
                                bool val = lift_node.AttachedElevator.Value.Value.IsClosed(lift_node.AttachedElevator.Value.Key);
                                if (val)
                                {
                                    lift_node.AttachedElevator.Value.Value.UseLift();

                                    Timing.KillCoroutines(MovementCoroutines.ToArray());
                                    Move(MovementDirection.NONE);

                                    while (!lift_node.AttachedElevator.Value.Value.operative)
                                    {
                                        yield return 0.0f;
                                    }

                                    GoTo(CurrentNavTarget.Position);
                                }
                            }
                        }

                        if (CurMovementDirection == MovementDirection.NONE)
                        {
                            //Target reached - force position to it so we wont stuck
                            Vector3 forced = new Vector3(CurrentNavTarget.Position.x, PlayerInstance.Position.y, CurrentNavTarget.Position.z);
                            PlayerInstance.ReferenceHub.playerMovementSync.OverridePosition(forced, 0f, true);

                            //If we have AI item target reached, try to find and take item
                            if (CurrentAIItemNodeTarget != null && CurrentAIItemNodeTarget == CurrentNavTarget)
                            {
                                CurrentAIItemNodeTarget = null;
                                IEnumerable<Pickup> pickups = FindObjectsOfType<Pickup>().Where(pk => Vector3.Distance(pk.Networkposition, PlayerInstance.Position) <= 5f);
                                foreach (Pickup p in pickups)
                                {
                                    if (Utils.Utils.CheckItemType(CurrentAIItemGroupTarget, p.ItemId))
                                    {
                                        yield return Timing.WaitForSeconds(GoTo(p.position));
                                        //TakeItem(p); TODO
                                        break;
                                    }
                                }
                                CurrentAIItemGroupTarget = null;
                            }
                            CurrentNavTarget = null;
                        }
                    }
                    else if (NavigationQueue.Count > 0)
                    {
                        //No current, but there are pending targets
                        CurrentNavTarget = NavigationQueue.First.Value;
                        NavigationQueue.RemoveFirst();
                        if (CurrentNavTarget.AttachedElevator != null && Math.Abs(CurrentNavTarget.AttachedElevator.Value.Key.target.position.y - PlayerInstance.Position.y) > 2f)
                        {
                            CurrentNavTarget.AttachedElevator.Value.Value.UseLift();
                            while (CurrentNavTarget.AttachedElevator.Value.Value.status == Lift.Status.Moving)
                            {
                                yield return 0f;
                            }

                            CurrentNavTarget = null;
                        }
                        else
                        {
                            GoTo(CurrentNavTarget.Position);
                        }
                    }
                    else if (CurrentAIRoomTarget != null)
                    {
                        //No current, no pending - room reached
                        CurrentAIRoomTarget = null;
                    }
                }
                yield return Timing.WaitForSeconds(Plugin.Instance.Config.NavUpdateFrequency);
            }
        }

        private IEnumerator<float> UpdateTalking()
        {
            List<Player> invalid_players = new List<Player>();
            for (; ; )
            {
                while (!IsValid)
                {
                    yield return 0.0f;
                }
                invalid_players.Clear();
                foreach (Player p in TalkingStates.Keys)
                {
                    if (!p.IsAlive || !Player.Dictionary.ContainsKey(p.GameObject))
                    {
                        invalid_players.Add(p);
                    }
                }
                foreach (Player p in invalid_players)
                {
                    TalkingStates.Remove(p);
                    if (p == LockHandler)
                    {
                        LockHandler = null;
                        IsLocked = false;
                    }
                }
                yield return Timing.WaitForSeconds(0.5f);
            }
        }

        private IEnumerator<float> MoveCoroutine()
        {
            for (; ; )
            {
                while (!IsValid)
                {
                    yield return 0.0f;
                }
                float speed = MovementSpeed;
                switch (CurMovementDirection)
                {
                    case MovementDirection.FORWARD:
                        try
                        {
                            if (!Physics.Linecast(PlayerInstance.Position, PlayerInstance.Position + PlayerInstance.CameraTransform.forward / 10 * speed, PlayerInstance.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                PlayerInstance.ReferenceHub.playerMovementSync.OverridePosition(PlayerInstance.Position + PlayerInstance.CameraTransform.forward / 10 * speed, 0f, true);
                            }
                        }
                        catch (Exception) { }
                        break;

                    case MovementDirection.BACKWARD:
                        try
                        {
                            if (!Physics.Linecast(PlayerInstance.Position, gameObject.transform.position - PlayerInstance.CameraTransform.forward / 10 * speed, PlayerInstance.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                PlayerInstance.ReferenceHub.playerMovementSync.OverridePosition(PlayerInstance.Position - PlayerInstance.CameraTransform.forward / 10 * speed, 0f, true);
                            }
                        }
                        catch (Exception) { }
                        break;

                    case MovementDirection.LEFT:
                        try
                        {
                            if (!Physics.Linecast(PlayerInstance.Position, PlayerInstance.Position + Quaternion.AngleAxis(90, Vector3.up) * PlayerInstance.CameraTransform.forward / 10 * speed, PlayerInstance.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                PlayerInstance.ReferenceHub.playerMovementSync.OverridePosition(PlayerInstance.Position + Quaternion.AngleAxis(90, Vector3.up) * PlayerInstance.CameraTransform.forward / 10 * speed, 0f, true);
                            }
                        }
                        catch (Exception) { }
                        break;

                    case MovementDirection.RIGHT:
                        try
                        {
                            if (!Physics.Linecast(PlayerInstance.Position, PlayerInstance.Position - Quaternion.AngleAxis(90, Vector3.up) * PlayerInstance.CameraTransform.forward / 10 * speed, PlayerInstance.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                PlayerInstance.ReferenceHub.playerMovementSync.OverridePosition(PlayerInstance.Position - Quaternion.AngleAxis(90, Vector3.up) * PlayerInstance.CameraTransform.forward / 10 * speed, 0f, true);
                            }
                        }
                        catch (Exception) { }

                        break;

                    default:
                        break;
                }
                yield return Timing.WaitForSeconds(Plugin.Instance.Config.MovementUpdateFrequency);
            }
        }

        public void Stop()
        {
            ClearNavTargets();
            FollowTarget = null;
            CurrentAIRoomTarget = null;
            CurrentAIItemNodeTarget = null;
            CurrentAIItemGroupTarget = null;
            Timing.KillCoroutines(MovementCoroutines.ToArray());
            Move(MovementDirection.NONE);
        }

        private IEnumerator<float> StartTalkCoroutine(Player p)
        {
            if (TalkingStates.ContainsKey(p))
            {
                p.SendConsoleMessage($"[{PlayerInstance.Nickname}] {Plugin.Instance.Translation.AlreadyTalking}", "yellow");
            }
            else
            {
                IsLocked = true;
                LockHandler = p;
                TalkingStates.Add(p, RootNode);
                bool end = RootNode.Send(PlayerInstance.Nickname, p);
                IsActionLocked = true;
                foreach (NodeAction action in RootNode.Actions.Keys)
                {
                    action.Process(this, p, RootNode.Actions[action]);
                    float dur = 0;
                    try
                    {
                        dur = float.Parse(RootNode.Actions[action]["next_action_delay"].Replace('.', ','));
                    }
                    catch (Exception) { }
                    yield return Timing.WaitForSeconds(dur);
                }
                IsActionLocked = false;
                if (end)
                {
                    TalkingStates.Remove(p);
                    p.SendConsoleMessage(PlayerInstance.Nickname + $" {Plugin.Instance.Translation.TalkEnd}", "yellow");
                    IsLocked = false;
                }
            }
        }

        private IEnumerator<float> HandleAnswerCoroutine(Player p, string answer)
        {
            if (TalkingStates.ContainsKey(p))
            {
                TalkNode cur_node = TalkingStates[p];
                if (int.TryParse(answer, out int node))
                {
                    if (cur_node.NextNodes.TryGet(node, out TalkNode new_node))
                    {
                        TalkingStates[p] = new_node;
                        IsActionLocked = true;
                        bool end = new_node.Send(PlayerInstance.Nickname, p);
                        foreach (NodeAction action in new_node.Actions.Keys)
                        {
                            try
                            {
                                action.Process(this, p, new_node.Actions[action]);
                            }
                            catch (Exception e)
                            {
                                Log.Error($"Exception during processing action {action.Name}: {e}");
                            }
                            float dur = 0;
                            try
                            {
                                dur = float.Parse(new_node.Actions[action]["next_action_delay"].Replace('.', ','));
                            }
                            catch (Exception) { }
                            yield return Timing.WaitForSeconds(dur);
                        }
                        IsActionLocked = false;
                        if (end)
                        {
                            TalkingStates.Remove(p);
                            p.SendConsoleMessage(PlayerInstance.Nickname + $" {Plugin.Instance.Translation.TalkEnd}", "yellow");
                            IsLocked = false;
                        }
                    }
                    else
                    {
                        p.SendConsoleMessage(Plugin.Instance.Translation.InvalidAnswer, "red");
                    }
                }
                else
                {
                    p.SendConsoleMessage(Plugin.Instance.Translation.IncorrectFormat, "red");
                }
            }
            else
            {
                p.SendConsoleMessage(Plugin.Instance.Translation.NotTalking, "red");
            }
        }

        #endregion Coroutines

        #region Movement

        public void Move(MovementDirection dir)
        {
            CurMovementDirection = dir;

            if (!DisableRun)
            {
                if (IsRunning)
                {
                    PlayerInstance.ReferenceHub.animationController.Network_curMoveState = (byte)PlayerMovementState.Sprinting;
                }
                else
                {
                    PlayerInstance.ReferenceHub.animationController.Network_curMoveState = (byte)PlayerMovementState.Walking;
                }
            }
            float speed = MovementSpeed;
            switch (dir)
            {
                case MovementDirection.FORWARD:
                    PlayerInstance.ReferenceHub.animationController.Networkspeed = new Vector2(speed, 0);
                    break;

                case MovementDirection.BACKWARD:
                    PlayerInstance.ReferenceHub.animationController.Networkspeed = new Vector2(-speed, 0);
                    break;

                case MovementDirection.RIGHT:
                    PlayerInstance.ReferenceHub.animationController.Networkspeed = new Vector2(0, speed);
                    break;

                case MovementDirection.LEFT:
                    PlayerInstance.ReferenceHub.animationController.Networkspeed = new Vector2(0, -speed);
                    break;

                default:
                    PlayerInstance.ReferenceHub.animationController.Networkspeed = new Vector2(0, 0);
                    break;
            }
        }

        public void AddNavTarget(NavigationNode node)
        {
            NavigationQueue.AddLast(node);
        }

        public void ClearNavTargets()
        {
            NavigationQueue.Clear();
            CurrentNavTarget = null;
        }

        public void Follow(Player p)
        {
            FollowTarget = p;
        }

        public float GoTo(Vector3 position)
        {
            IsActionLocked = true;
            Timing.KillCoroutines(MovementCoroutines.ToArray());
            Vector3 heading = (position - PlayerInstance.Position);
            heading.y = 0;
            Quaternion lookRot = Quaternion.LookRotation(heading.normalized);
            float dist = heading.magnitude;
            PlayerInstance.Rotations = new Vector2(lookRot.eulerAngles.x, lookRot.eulerAngles.y);
            Move(MovementDirection.FORWARD);
            float eta = Plugin.Instance.Config.MovementUpdateFrequency * (dist / (PlayerInstance.CameraTransform.forward / 10 * MovementSpeed).magnitude);
            position.y = PlayerInstance.Position.y;
            MovementCoroutines.Add(Timing.CallDelayed(eta, () =>
            {
                Move(MovementDirection.NONE);
                if (Vector3.Distance(PlayerInstance.Position, position) >= 2f)
                {
                    PlayerInstance.ReferenceHub.playerMovementSync.OverridePosition(position, 0f, true);
                }
                IsActionLocked = false;
            }));
            return eta;
        }

        #endregion Movement

        #region Navigation

        private void TryProcessNode(NavigationNode target, NavigationNode current, int prev_value, ref Dictionary<NavigationNode, int> visited_nodes)
        {
            if (Map.IsLCZDecontaminated && current.Position.y < 200f && current.Position.y > -200f)
            {
                visited_nodes.Add(current, int.MinValue);
                return;
            }
            visited_nodes.Add(current, prev_value + 1);
            if (current == target)
            {
                return;
            }
            foreach (NavigationNode node in current.LinkedNodes)
            {
                if (visited_nodes.ContainsKey(node))
                {
                    continue;
                }
                TryProcessNode(target, node, prev_value + 1, ref visited_nodes);
            }
        }

        private NavigationNode FindNextNode(NavigationNode current, Dictionary<NavigationNode, int> visited)
        {
            foreach (NavigationNode node in current.LinkedNodes)
            {
                if (visited.ContainsKey(node) && visited[node] == visited[current] - 1)
                {
                    return node;
                }
            }
            return null;
        }

        public bool GotoNode(NavigationNode target_node)
        {
            NavigationNode nearest_node = null;
            float min_dist = float.MaxValue;
            foreach (NavigationNode node in NavigationNode.AllNodes.Values)
            {
                if (node.LinkedNodes.Count > 0)
                {
                    float new_dist = Vector3.Distance(PlayerInstance.Position, node.Position);
                    if (new_dist < min_dist)
                    {
                        min_dist = new_dist;
                        nearest_node = node;
                    }
                }
            }
            if (nearest_node != null)
            {
                Log.Debug($"[NAV] Selected nearest node: {nearest_node.Name}", Plugin.Instance.Config.VerboseOutput);

                Dictionary<NavigationNode, int> visited = new Dictionary<NavigationNode, int>();
                TryProcessNode(target_node, nearest_node, -1, ref visited);
                if (!visited.ContainsKey(target_node))
                {
                    Log.Debug("[NAV] Failed to build way", Plugin.Instance.Config.VerboseOutput);
                    return false;
                }
                else
                {
                    NavigationQueue.Clear();
                    NavigationNode cur = target_node;
                    Log.Debug("[NAV] Built way:", Plugin.Instance.Config.VerboseOutput);
                    do
                    {
                        Log.Debug($"[NAV] {cur.Name}", Plugin.Instance.Config.VerboseOutput);
                        NavigationQueue.AddFirst(cur);
                        cur = FindNextNode(cur, visited);
                    } while (cur != null);

                    return true;
                }
            }
            else
            {
                Log.Error("[NAV] Failed to find nearest navnode");
                return false;
            }
        }

        public bool GotoRoom(Room r)
        {
            NavigationNode target_node = NavigationNode.FromRoom(r);
            if (target_node != null)
            {
                return GotoNode(target_node);
            }
            else
            {
                Log.Debug("[NAV] Specified room has null navnode", Plugin.Instance.Config.VerboseOutput);
                return false;
            }
        }

        #endregion Navigation

        #region API

        public bool DisableDialogSystem { get; set; } = false;

        #endregion API

        public void TalkWith(Player p)
        {
            AttachedCoroutines.Add(Timing.RunCoroutine(StartTalkCoroutine(p)));
        }

        public void HandleAnswer(Player p, string answer)
        {
            if (!AIEnabled && !IsActionLocked)
            {
                AttachedCoroutines.Add(Timing.RunCoroutine(HandleAnswerCoroutine(p, answer)));
            }
            else
            {
                p.SendConsoleMessage($"[{PlayerInstance.Nickname}] {Plugin.Instance.Translation.NpcBusy}", "yellow");
            }
        }

        public void FireEvent(string eventName, Dictionary<string, object> args)
        {
            if(ScriptScope == null)
            {
                Log.Debug("Skipping event process: ScriptScope == null", Plugin.Instance.Config.VerboseOutput);
                return;
            }
            args["npc"] = AIController;
            args["helper"] = AIHelper;
            try
            {
                Func<Dictionary<string, object>, bool> eventHandler = ScriptScope.GetVariable<Func<Dictionary<string, object>, bool>>(eventName + "Handler");
                eventHandler.Invoke(args);
            }catch(MissingMemberException)
            {
                Log.Debug($"[{GetIdentifier()}({PlayerInstance.Nickname})] Skipping unused event: {eventName}", Plugin.Instance.Config.VerboseOutput);
            }catch(Exception e)
            {
                Log.Error($"Error occured during handling event {eventName} in {GetIdentifier()}({PlayerInstance.Nickname}): {e}");
            }
        }

        public override string GetIdentifier()
        {
            return $"{Plugin.Instance.Name}_NPC_{__counter}";
        }

        public override bool IsVisibleFor(Player ply)
        {
            return true;
        }

        public override void OnPostInitialization()
        {
            __counter++;
            PlayerInstance.SessionVariables.Add("IsNPC", true);
            AIController = new NPCAIController(this);
            AIHelper = new NPCAIHelper(this);
            AttachedCoroutines.Add(Timing.RunCoroutine(UpdateTalking()));
            AttachedCoroutines.Add(Timing.RunCoroutine(MoveCoroutine()));
            AttachedCoroutines.Add(Timing.RunCoroutine(NavCoroutine()));
        }

        public override void OnPreInitialization()
        {
        }

        public override void OnDestroying()
        {
            Timing.KillCoroutines(MovementCoroutines.ToArray());
        }

        public override bool DisplayInRA { get; set; } = Plugin.Instance.Config.DisplayNpcInRemoteAdmin;
    }
}