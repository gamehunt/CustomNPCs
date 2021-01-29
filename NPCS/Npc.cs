using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using Microsoft.Scripting.Hosting;
using NPCS.AI;
using NPCS.AI.Python;
using NPCS.Events;
using NPCS.Navigation;
using NPCS.Talking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

namespace NPCS
{
    public class Npc : MonoBehaviour
    {
        #region Serialization

        //Megabruh
        private class NPCMappingInfo
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
                    return parent?.NPCPlayer.CurrentRoom.Name ?? deserializedRoom;
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
                    return parent?.NPCPlayer.CurrentRoom.Transform.localRotation.eulerAngles.y ?? deserializedRoomRotation;
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
                    Vector3? source = parent?.NPCPlayer.Position - parent?.NPCPlayer.CurrentRoom.Position;
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
                    Vector2? source = parent?.NPCPlayer.Rotations;
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

        public static IEnumerable<Npc> List
        {
            get
            {
                return Dictionary.Values;
            }
        }

        public static Dictionary<GameObject, Npc> Dictionary { get; } = new Dictionary<GameObject, Npc>();

        public Player NPCPlayer { get; set; }

        public TalkNode RootNode { get; set; }

        public string SaveFile { get; set; } = null;

        public string Name
        {
            get
            {
                return NPCPlayer.Nickname;
            }
        }

        public Dictionary<Player, TalkNode> TalkingStates { get; } = new Dictionary<Player, TalkNode>();

        public ItemType ItemHeld
        {
            get
            {
                return NPCPlayer.Inventory.curItem;
            }
            set
            {
                NPCPlayer.Inventory.curItem = value; ;
                if (value != ItemType.None)
                {
                    if (!AvailableItems.Contains(value))
                    {
                        TakeItem(value);
                    }
                }
            }
        }

        public MovementDirection CurMovementDirection { get; set; }

        public bool IsLocked { get; set; } = false;

        public Player LockHandler { get; set; } = null;

        public bool IsActionLocked { get; set; } = false;

        public bool IsExclusive { get; set; } = false;

        public Dictionary<string, List<KeyValuePair<NodeAction, Dictionary<string, string>>>> Events { get; } = new Dictionary<string, List<KeyValuePair<NodeAction, Dictionary<string, string>>>>();

        public LinkedList<NavigationNode> NavigationQueue { get; private set; } = new LinkedList<NavigationNode>();

        public NavigationNode CurrentNavTarget { get; set; } = null;

        public Player FollowTarget { get; set; } = null;

        public TargetLostBehaviour OnTargetLostBehaviour { get; set; } = TargetLostBehaviour.TELEPORT;

        private float __customSpeed = 0f;

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
                    return IsRunning ? CharacterClassManager._staticClasses[(int)NPCPlayer.Role].runSpeed : CharacterClassManager._staticClasses[(int)NPCPlayer.Role].walkSpeed;
                }
            }
            set
            {
                __customSpeed = value;
            }
        }

        public bool DisableRun { get; set; } = false;
        public bool IsRunning { get; set; } = false;

        public bool ProcessSCPLogic { get; set; } = false;

        public List<CoroutineHandle> AttachedCoroutines { get; } = new List<CoroutineHandle>();

        public List<CoroutineHandle> MovementCoroutines { get; } = new List<CoroutineHandle>();

        //AI STATES -------------------------------
        public bool AIEnabled { get; set; } = false;

        public Utils.AIMode AIMode { get; set; } = Utils.AIMode.Legacy;
        public List<string> AIScripts { get; set; } = new List<string>();

        // ============ Legacy
        public LinkedList<AITarget> AIQueue { get; private set; } = new LinkedList<AITarget>();

        public AITarget CurrentAITarget { get; set; } = null;
        public Player CurrentAIPlayerTarget { get; set; } = null;
        public Room CurrentAIRoomTarget { get; set; } = null;
        public string CurrentAIItemGroupTarget { get; set; } = null;
        public NavigationNode CurrentAIItemNodeTarget { get; set; } = null;
        public int SkippedTargets { get; set; } = 0;

        // ============ Python
        public NPCAIController AIController { get; set; } = null;

        public NPCAIHelper AIHelper { get; set; } = null;
        //------------------------------------------

        //Inventory --------------------------------
        public ItemType[] AvailableItems { get; set; } = new ItemType[8] { ItemType.None, ItemType.None, ItemType.None, ItemType.None, ItemType.None, ItemType.None, ItemType.None, ItemType.None };

        public int FreeSlots
        {
            get
            {
                return AvailableItems.Where(it => it == ItemType.None).Count();
            }
        }

        public Dictionary<ItemType, int> AvailableWeapons { get; } = new Dictionary<ItemType, int>();

        public ItemType[] AvailableKeycards
        {
            get
            {
                return AvailableItems.Where(it => it.IsKeycard()).ToArray();
            }
        }

        //------------------------------------------

        #endregion Properties

        #region Coroutines

        public void StartAI()
        {
            Log.Info($"Starting AI with mode: {AIMode:g}({AIEnabled})");
            AttachedCoroutines.Add(Timing.RunCoroutine(AICoroutine()));
        }

        private IEnumerator<float> AICoroutine()
        {
            if (AIEnabled)
            {
                if (AIMode != Utils.AIMode.Python)
                {
                    for (; ; )
                    {
                        if (CurrentAITarget != null)
                        {
                            bool res = false;
                            try
                            {
                                res = CurrentAITarget.Check(this);
                            }
                            catch (Exception e)
                            {
                                Log.Warn($"AI Target check failure: {e}");
                            }
                            if (res)
                            {
                                float delay = 0f;
                                bool failure = false;
                                try
                                {
                                    delay = CurrentAITarget.Process(this);
                                    for (; SkippedTargets > 0; SkippedTargets--)
                                    {
                                        AITarget target = AIQueue.First.Value;
                                        AIQueue.RemoveFirst();
                                        AIQueue.AddLast(target);
                                    }
                                }
                                catch (Exception e)
                                {
                                    failure = true;
                                    Log.Warn($"Target processing failure: {e}");
                                }

                                yield return Timing.WaitForSeconds(delay);

                                if (CurrentAITarget.IsFinished || failure)
                                {
                                    CurrentAITarget.IsFinished = false;
                                    CurrentAITarget = null;
                                }
                            }
                            else
                            {
                                CurrentAITarget.IsFinished = false;
                                CurrentAITarget = null;
                                yield return Timing.WaitForSeconds(Plugin.Instance.Config.AIIdleUpdateFrequency);
                            }
                        }
                        else
                        {
                            try
                            {
                                if (!AIQueue.IsEmpty())
                                {
                                    CurrentAITarget = AIQueue.First.Value;
                                    AIQueue.RemoveFirst();
                                    AIQueue.AddLast(CurrentAITarget);
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Debug($"Error while scheduling AI target: {e}", Plugin.Instance.Config.VerboseOutput);
                            }
                            yield return Timing.WaitForSeconds(Plugin.Instance.Config.AIIdleUpdateFrequency);
                        }
                    }
                }
                else
                {
                    Log.Info("Switched to python mode...");
                    ScriptScope scope = Plugin.Engine.CreateScope();
                    scope.SetVariable("controller", AIController);
                    scope.SetVariable("helper", AIHelper);
                    for (; ; )
                    {
                        if (AIScripts.Count != 0)
                        {
                            foreach (string script in AIScripts)
                            {
                                //Log.Debug($"Running script {script}", Plugin.Instance.Config.VerboseOutput);
                                float delay = 0f;
                                try
                                {
                                    Plugin.Engine.ExecuteFile(script, scope);
                                    delay = scope.GetVariable<float>("delay");
                                }
                                catch (Exception e)
                                {
                                    Log.Error($"AI script failure: {e}");
                                }
                                //Log.Debug($"--> {delay}", Plugin.Instance.Config.VerboseOutput);
                                yield return Timing.WaitForSeconds(delay);
                            }
                        }
                        else
                        {
                            yield return Timing.WaitForSeconds(Plugin.Instance.Config.AIIdleUpdateFrequency);
                        }
                    }
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
                if (FollowTarget != null)
                {
                    if (FollowTarget.IsAlive)
                    {
                        float dist = Vector3.Distance(FollowTarget.Position, NPCPlayer.Position);

                        //If we are far away...
                        if (dist >= Plugin.Instance.Config.MaxFollowDistance)
                        {
                            if (OnTargetLostBehaviour == TargetLostBehaviour.TELEPORT)
                            {
                                //... Teleport to player if allowed
                                NPCPlayer.Position = FollowTarget.Position;
                                eta = 0;
                                FollowTargetPosCache.Clear();
                            }
                            else
                            {
                                //Stop or try to search otherwise

                                FireEvent(new NPCTargetLostEvent(this, FollowTarget));

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
                        FireEvent(new NPCFollowTargetDiedEvent(this, FollowTarget));
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
                        float distance = Vector3.Distance(CurrentNavTarget.Position, NPCPlayer.Position);

                        if (distance < 3f && (!ProcessSCPLogic || NPCPlayer.Role != RoleType.Scp106))
                        {
                            //Try to open the door if there is one, so we wont collide with it
                            if (CurrentNavTarget.AttachedDoor != null && !CurrentNavTarget.AttachedDoor.NetworkTargetState)
                            {
                                ItemType prev = ItemHeld;
                                bool open = CurrentNavTarget.AttachedDoor.RequiredPermissions.CheckPermissions(prev, NPCPlayer.ReferenceHub);
                                if (!open)
                                {
                                    foreach (ItemType keycard in AvailableKeycards)
                                    {
                                        if (CurrentNavTarget.AttachedDoor.RequiredPermissions.CheckPermissions(keycard, NPCPlayer.ReferenceHub))
                                        {
                                            ItemHeld = keycard;
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
                                    ItemHeld = prev;
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
                            Vector3 forced = new Vector3(CurrentNavTarget.Position.x, NPCPlayer.Position.y, CurrentNavTarget.Position.z);
                            NPCPlayer.ReferenceHub.playerMovementSync.OverridePosition(forced, 0f, true);

                            //If we have AI item target reached, try to find and take item
                            if (CurrentAIItemNodeTarget != null && CurrentAIItemNodeTarget == CurrentNavTarget)
                            {
                                CurrentAIItemNodeTarget = null;
                                IEnumerable<Pickup> pickups = FindObjectsOfType<Pickup>().Where(pk => Vector3.Distance(pk.Networkposition, NPCPlayer.Position) <= 5f);
                                foreach (Pickup p in pickups)
                                {
                                    if (Utils.Utils.CheckItemType(CurrentAIItemGroupTarget, p.ItemId))
                                    {
                                        yield return Timing.WaitForSeconds(GoTo(p.position));
                                        TakeItem(p);
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
                        if (CurrentNavTarget.AttachedElevator != null && Math.Abs(CurrentNavTarget.AttachedElevator.Value.Key.target.position.y - NPCPlayer.Position.y) > 2f)
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
                float speed = MovementSpeed;
                switch (CurMovementDirection)
                {
                    case MovementDirection.FORWARD:
                        try
                        {
                            if (!Physics.Linecast(NPCPlayer.Position, NPCPlayer.Position + NPCPlayer.CameraTransform.forward / 10 * speed, NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                NPCPlayer.ReferenceHub.playerMovementSync.OverridePosition(NPCPlayer.Position + NPCPlayer.CameraTransform.forward / 10 * speed, 0f, true);
                            }
                        }
                        catch (Exception) { }
                        break;

                    case MovementDirection.BACKWARD:
                        try
                        {
                            if (!Physics.Linecast(NPCPlayer.Position, gameObject.transform.position - NPCPlayer.CameraTransform.forward / 10 * speed, NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                NPCPlayer.ReferenceHub.playerMovementSync.OverridePosition(NPCPlayer.Position - NPCPlayer.CameraTransform.forward / 10 * speed, 0f, true);
                            }
                        }
                        catch (Exception) { }
                        break;

                    case MovementDirection.LEFT:
                        try
                        {
                            if (!Physics.Linecast(NPCPlayer.Position, NPCPlayer.Position + Quaternion.AngleAxis(90, Vector3.up) * NPCPlayer.CameraTransform.forward / 10 * speed, NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                NPCPlayer.ReferenceHub.playerMovementSync.OverridePosition(NPCPlayer.Position + Quaternion.AngleAxis(90, Vector3.up) * NPCPlayer.CameraTransform.forward / 10 * speed, 0f, true);
                            }
                        }
                        catch (Exception) { }
                        break;

                    case MovementDirection.RIGHT:
                        try
                        {
                            if (!Physics.Linecast(NPCPlayer.Position, NPCPlayer.Position - Quaternion.AngleAxis(90, Vector3.up) * NPCPlayer.CameraTransform.forward / 10 * speed, NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                NPCPlayer.ReferenceHub.playerMovementSync.OverridePosition(NPCPlayer.Position - Quaternion.AngleAxis(90, Vector3.up) * NPCPlayer.CameraTransform.forward / 10 * speed, 0f, true);
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
                p.SendConsoleMessage($"[{Name}] {Plugin.Instance.Config.TranslationAlreadyTalking}", "yellow");
            }
            else
            {
                IsLocked = true;
                LockHandler = p;
                TalkingStates.Add(p, RootNode);
                bool end = RootNode.Send(Name, p);
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
                    p.SendConsoleMessage(Name + $" {Plugin.Instance.Config.TranslationTalkEnd}", "yellow");
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
                        bool end = new_node.Send(Name, p);
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
                            p.SendConsoleMessage(Name + $" {Plugin.Instance.Config.TranslationTalkEnd}", "yellow");
                            IsLocked = false;
                        }
                    }
                    else
                    {
                        p.SendConsoleMessage(Plugin.Instance.Config.TranslationInvalidAnswer, "red");
                    }
                }
                else
                {
                    p.SendConsoleMessage(Plugin.Instance.Config.TranslationIncorrectFormat, "red");
                }
            }
            else
            {
                p.SendConsoleMessage(Plugin.Instance.Config.TranslationNotTalking, "red");
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
                    NPCPlayer.ReferenceHub.animationController.Network_curMoveState = (byte)PlayerMovementState.Sprinting;
                }
                else
                {
                    NPCPlayer.ReferenceHub.animationController.Network_curMoveState = (byte)PlayerMovementState.Walking;
                }
            }
            float speed = MovementSpeed;
            switch (dir)
            {
                case MovementDirection.FORWARD:
                    NPCPlayer.ReferenceHub.animationController.Networkspeed = new Vector2(speed, 0);
                    break;

                case MovementDirection.BACKWARD:
                    NPCPlayer.ReferenceHub.animationController.Networkspeed = new Vector2(-speed, 0);
                    break;

                case MovementDirection.RIGHT:
                    NPCPlayer.ReferenceHub.animationController.Networkspeed = new Vector2(0, speed);
                    break;

                case MovementDirection.LEFT:
                    NPCPlayer.ReferenceHub.animationController.Networkspeed = new Vector2(0, -speed);
                    break;

                default:
                    NPCPlayer.ReferenceHub.animationController.Networkspeed = new Vector2(0, 0);
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
            Vector3 heading = (position - NPCPlayer.Position);
            heading.y = 0;
            Quaternion lookRot = Quaternion.LookRotation(heading.normalized);
            float dist = heading.magnitude;
            NPCPlayer.Rotations = new Vector2(lookRot.eulerAngles.x, lookRot.eulerAngles.y);
            Move(MovementDirection.FORWARD);
            float eta = Plugin.Instance.Config.MovementUpdateFrequency * (dist / (NPCPlayer.CameraTransform.forward / 10 * MovementSpeed).magnitude);
            position.y = NPCPlayer.Position.y;
            MovementCoroutines.Add(Timing.CallDelayed(eta, () =>
            {
                Move(MovementDirection.NONE);
                if (Vector3.Distance(NPCPlayer.Position, position) >= 2f)
                {
                    NPCPlayer.ReferenceHub.playerMovementSync.OverridePosition(position, 0f, true);
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
                    float new_dist = Vector3.Distance(NPCPlayer.Position, node.Position);
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

        public HashSet<RoleType> VisibleForRoles { get; set; } = new HashSet<RoleType>();
        public HashSet<Player> VisibleForPlayers { get; set; } = new HashSet<Player>();
        public bool ShouldTrigger096 { get; set; } = false;

        public bool DontCleanup { get; set; } = false;

        public bool AffectRoundSummary { get; set; } = false;

        public bool IsValid { get; set; } = true;

        #endregion API

        public void TakeItem(Pickup item)
        {
            /* if (item.ItemId.IsAmmo())
             {
                 uint delta = 0;
                 uint limit;
                 switch (item.ItemId)
                 {
                     case ItemType.Ammo556:
                         limit = NPCPlayer.ReferenceHub.searchCoordinator.ConfigPipe.GetLimitAmmo((byte)AmmoType.Nato556);
                         NPCPlayer.Ammo[(int)AmmoType.Nato556] += (uint)item.durability;
                         if (NPCPlayer.Ammo[(int)AmmoType.Nato556] > limit)
                         {
                             delta = NPCPlayer.Ammo[(int)AmmoType.Nato556] - limit;
                             NPCPlayer.Ammo[(int)AmmoType.Nato556] = limit;
                         }
                         break;

                     case ItemType.Ammo762:
                         limit = NPCPlayer.ReferenceHub.searchCoordinator.ConfigPipe.GetLimitAmmo((byte)AmmoType.Nato762);
                         NPCPlayer.Ammo[(int)AmmoType.Nato762] += (uint)item.durability;
                         if (NPCPlayer.Ammo[(int)AmmoType.Nato762] > limit)
                         {
                             delta = NPCPlayer.Ammo[(int)AmmoType.Nato762] - limit;
                             NPCPlayer.Ammo[(int)AmmoType.Nato762] = limit;
                         }
                         break;

                     case ItemType.Ammo9mm:
                         limit = NPCPlayer.ReferenceHub.searchCoordinator.ConfigPipe.GetLimitAmmo((byte)AmmoType.Nato9);
                         NPCPlayer.Ammo[(int)AmmoType.Nato9] += (uint)item.durability;
                         if (NPCPlayer.Ammo[(int)AmmoType.Nato9] > limit)
                         {
                             delta = NPCPlayer.Ammo[(int)AmmoType.Nato9] - limit;
                             NPCPlayer.Ammo[(int)AmmoType.Nato9] = limit;
                         }
                         break;
                 }
                 if (delta > 0)
                 {
                     item.durability = delta;
                 }
                 else
                 {
                     item.Delete();
                 }
             }
             else
             {*/
            if (FreeSlots > 0) // If there are free slots...
            {
                //Take it
                int free_slot = AvailableItems.IndexOf(ItemType.None);
                AvailableItems[free_slot] = item.itemId;
                if (item.itemId.IsWeapon())
                {
                    AvailableWeapons.Add(item.itemId, (int)item.durability);
                }
                item.Delete();
            }
            else //Otherwise we are probably went there from smart target...
            {
                //Try drop old item and take new one
                if (CurrentAIItemGroupTarget == "keycard" && AvailableKeycards.Length != 0)
                {
                    DropItem(AvailableKeycards[0], true);
                    TakeItem(item);
                    item.Delete();
                }
                else if (CurrentAIItemGroupTarget == "weapon" && AvailableWeapons.Count != 0)
                {
                    DropItem(AvailableWeapons.Keys.ElementAt(0), true);
                    TakeItem(item);
                    AvailableWeapons[item.itemId] = (int)item.durability;
                    item.Delete();
                }
            }
            //  }
        }

        public void TakeItem(ItemType item)
        {
            if (item == ItemType.None)
            {
                return;
            }
            if (FreeSlots > 0)
            {
                int free_slot = AvailableItems.IndexOf(ItemType.None);
                AvailableItems[free_slot] = item;
                if (item.IsWeapon())
                {
                    WeaponManager.Weapon[] weapons = NPCPlayer.ReferenceHub.weaponManager.weapons;
                    for (int i = 0; i < weapons.Length; i++)
                    {
                        if (weapons[i].inventoryID == item)
                        {
                            AvailableWeapons.Add(item, (int)NPCPlayer.ReferenceHub.weaponManager.weapons[i].maxAmmo);
                            break;
                        }
                    }
                }
            }
        }

        public void DropItem(ItemType type, bool spawn_drop)
        {
            if (AvailableItems.Contains(type))
            {
                int slot = AvailableItems.IndexOf(type);
                AvailableItems[slot] = ItemType.None;
                if (spawn_drop)
                {
                    Pickup pick = type.Spawn(1f, NPCPlayer.Position + new Vector3(0, 0.5f, 0));
                    if (AvailableWeapons.ContainsKey(type))
                    {
                        pick.durability = AvailableWeapons[type];
                        AvailableWeapons.Remove(type);
                    }
                }
            }
        }

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
                p.SendConsoleMessage($"[{Name}] {Plugin.Instance.Config.TranslationNpcBusy}", "yellow");
            }
        }

        public void Kill(bool spawn_ragdoll)
        {
            if (IsValid)
            {
                IsValid = false;
                Log.Debug($"kill() called in NPC {Name}", Plugin.Instance.Config.VerboseOutput);
                if (spawn_ragdoll)
                {
                    gameObject.GetComponent<RagdollManager>().SpawnRagdoll(gameObject.transform.position, gameObject.transform.rotation, Vector3.zero, (int)NPCPlayer.Role, new PlayerStats.HitInfo(), false, "", Name, 9999);
                }
                UnityEngine.Object.Destroy(NPCPlayer.GameObject);
            }
        }

        public void FireEvent(NPCEvent ev)
        {
            if (!IsValid)
            {
                Log.Debug($"Skipping event {ev.Name} on invalidated NPC", Plugin.Instance.Config.VerboseOutput);
                return;
            }
            try
            {
                Log.Debug($"Fired event {ev.Name}", Plugin.Instance.Config.VerboseOutput);
                ev.FireActions(Events[ev.Name]);
                ev.OnFired(this);
            }
            catch (KeyNotFoundException)
            {
                Log.Debug($"Skipping unused event {ev.Name}", Plugin.Instance.Config.VerboseOutput);
            }
        }

        private void OnDestroy()
        {
            Dictionary.Remove(this.gameObject);
            Timing.KillCoroutines(MovementCoroutines.ToArray());
            Timing.KillCoroutines(AttachedCoroutines.ToArray());

            Log.SendRaw($"NPC {NPCPlayer.Nickname} ({NPCPlayer.Id}) deconstructed", ConsoleColor.Green);

            Player.IdsCache.Remove(NPCPlayer.Id);
            Player.Dictionary.Remove(NPCPlayer.GameObject);
        }

        public static Npc Get(Player p)
        {
            if (Dictionary.TryGetValue(p.GameObject, out Npc npc))
            {
                return npc;
            }
            else
            {
                return null;
            }
        }

        public static Npc Get(GameObject p)
        {
            if (Dictionary.TryGetValue(p, out Npc npc))
            {
                return npc;
            }
            else
            {
                return null;
            }
        }

        private void Awake()
        {
            NPCPlayer = Player.Get(gameObject);
            AIController = new NPCAIController(this);
            AIHelper = new NPCAIHelper(this);
            AttachedCoroutines.Add(Timing.RunCoroutine(UpdateTalking()));
            AttachedCoroutines.Add(Timing.RunCoroutine(MoveCoroutine()));
            AttachedCoroutines.Add(Timing.RunCoroutine(NavCoroutine()));
            Dictionary.Add(gameObject, this);
            Log.Debug($"Constructed NPC", Plugin.Instance.Config.VerboseOutput);
        }

        public static void SaveNPCMappings(string path)
        {
            path = Path.Combine(Config.NPCs_mappings_path, path);
            StreamWriter sw;
            if (!File.Exists(path))
            {
                sw = File.CreateText(path);
                var serializer = new SerializerBuilder().Build();
                List<NPCMappingInfo> infos = new List<NPCMappingInfo>();
                foreach (Npc n in Npc.List)
                {
                    if (n.SaveFile != null)
                    {
                        infos.Add(new NPCMappingInfo(n));
                    }
                }
                var yaml = serializer.Serialize(infos);
                sw.Write(yaml);
                sw.Close();
            }
            else
            {
                Log.Error("Failed to save npc mappings: File exists!");
            }
        }

        private static IEnumerator<float> NPCMappingsLoadCoroutine(List<NPCMappingInfo> infos)
        {
            foreach (NPCMappingInfo info in infos)
            {
                Room rm = Map.Rooms.Where(r => r.Name.Equals(info.Room, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (rm != null)
                {
                    Methods.CreateNPC(rm.Position + Quaternion.Euler(0, rm.Transform.localRotation.eulerAngles.y - info.RoomRotation, 0) * info.Relative.ToVector3(), info.Rotation.ToVector2() + new Vector2(0, rm.Transform.localRotation.eulerAngles.y - info.RoomRotation), info.File);
                    yield return Timing.WaitForSeconds(0.1f);
                }
            }
        }

        public static void LoadNPCMappings(string path)
        {
            path = Path.Combine(Config.NPCs_mappings_path, path);
            StreamReader sr;
            if (File.Exists(path))
            {
                sr = File.OpenText(path);
                var deserializer = new DeserializerBuilder().Build();
                List<NPCMappingInfo> infos = deserializer.Deserialize<List<NPCMappingInfo>>(sr);
                sr.Close();
                if (infos != null)
                {
                    Timing.RunCoroutine(NPCMappingsLoadCoroutine(infos));
                }
                else
                {
                    Log.Error("Failed to load npc mappings: Format error!");
                }
            }
            else
            {
                Log.Error("Failed to load npc mappings: File not exists!");
            }
        }
    }
}