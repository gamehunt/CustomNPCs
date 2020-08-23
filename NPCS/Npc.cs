using Exiled.API.Features;
using MEC;
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
    internal class Npc : MonoBehaviour
    {
        #region Serialization

        private class NPCSerializeInfo
        {
            private readonly Npc parent;

            public NPCSerializeInfo(Npc which)
            {
                parent = which;
            }

            public string name
            {
                get
                {
                    return parent.Name;
                }
            }

            public int role
            {
                get
                {
                    return (int)parent.NPCPlayer.Role;
                }
            }

            public int item_held
            {
                get
                {
                    return (int)parent.ItemHeld;
                }
            }

            public string root_node
            {
                get
                {
                    return Path.GetFileName(parent.RootNode.NodeFile);
                }
            }

            public bool god_mode
            {
                get
                {
                    return parent.NPCPlayer.IsGodModeEnabled;
                }
            }

            public bool is_exclusive
            {
                get
                {
                    return parent.IsExclusive;
                }
            }
        }

        public void Serialize(string path)
        {
            path = Path.Combine(Config.NPCs_root_path, path);
            StreamWriter sw;
            if (!File.Exists(path))
            {
                sw = File.CreateText(path);
                var serializer = new SerializerBuilder().Build();
                NPCSerializeInfo info = new NPCSerializeInfo(this);
                var yaml = serializer.Serialize(info);
                sw.Write(yaml);
                sw.Close();
            }
            else
            {
                Log.Error("Failed to save npc: File exists!");
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

        public Player NPCPlayer { get; set; }

        public TalkNode RootNode { get; set; }

        public string Name
        {
            get
            {
                return NPCPlayer.ReferenceHub.nicknameSync.Network_myNickSync;
            }
            set
            {
                NPCPlayer.ReferenceHub.nicknameSync.Network_myNickSync = value;
            }
        }

        public Dictionary<Player, TalkNode> TalkingStates { get; } = new Dictionary<Player, TalkNode>();

        public ItemType ItemHeld
        {
            get
            {
                return NPCPlayer.ReferenceHub.inventory.curItem;
            }
            set
            {
                NPCPlayer.ReferenceHub.inventory.SetCurItem(value);
            }
        }

        public MovementDirection CurMovementDirection { get; set; }

        public bool IsLocked { get; set; } = false;

        public Player LockHandler { get; set; } = null;

        public bool IsActionLocked { get; set; } = false;

        public bool IsExclusive { get; set; } = false;

        public Dictionary<string, Dictionary<NodeAction, Dictionary<string, string>>> Events { get; } = new Dictionary<string, Dictionary<NodeAction, Dictionary<string, string>>>();

        public Queue<NavigationNode> NavigationQueue { get; } = new Queue<NavigationNode>();

        public NavigationNode CurrentNavTarget { get; set; } = null;

        public Player FollowTarget { get; set; } = null;

        public float MovementSpeed { get; set; } = 2f;

        public List<CoroutineHandle> AttachedCoroutines { get; } = new List<CoroutineHandle>();

        public List<CoroutineHandle> MovementCoroutines { get; } = new List<CoroutineHandle>();

        #endregion Properties

        #region Coroutines

        private IEnumerator<float> NavCoroutine()
        {
            for (; ; )
            {
                if (FollowTarget != null)
                {
                    if (FollowTarget.IsAlive)
                    {
                        GoTo(FollowTarget.Position);
                        if(Vector3.Distance(FollowTarget.Position,NPCPlayer.Position) >= 15f)
                        {
                            NPCPlayer.Position = FollowTarget.Position;
                        }
                    }
                    else
                    {
                        FollowTarget = null;
                    }
                }
                else
                {
                    if (!NavigationQueue.IsEmpty())
                    {
                        CurrentNavTarget = NavigationQueue.Dequeue();
                        yield return Timing.WaitForSeconds(GoTo(CurrentNavTarget.Position) + 0.1f);
                        CurrentNavTarget = null;
                    }
                }
                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        private IEnumerator<float> UpdateTalking()
        {
            for (; ; )
            {
                List<Player> invalid_players = new List<Player>();
                foreach (Player p in TalkingStates.Keys)
                {
                    if (!p.IsAlive || !Player.List.Contains(p))
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
                switch (CurMovementDirection)
                {
                    case MovementDirection.FORWARD:
                        try
                        {
                            if (!Physics.Linecast(NPCPlayer.Position, NPCPlayer.Position + NPCPlayer.CameraTransform.forward / 10 * MovementSpeed, NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                NPCPlayer.Position += NPCPlayer.CameraTransform.forward / 10 * MovementSpeed;
                            }
                        }
                        catch (Exception) { }
                        break;

                    case MovementDirection.BACKWARD:
                        try
                        {
                            if (!Physics.Linecast(NPCPlayer.Position, gameObject.transform.position - NPCPlayer.CameraTransform.forward / 10 * MovementSpeed, NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                NPCPlayer.Position -= NPCPlayer.CameraTransform.forward / 10 * MovementSpeed;
                            }
                        }
                        catch (Exception) { }
                        break;

                    case MovementDirection.LEFT:
                        try
                        {
                            if (!Physics.Linecast(NPCPlayer.Position, NPCPlayer.Position + Quaternion.AngleAxis(90, Vector3.up) * NPCPlayer.CameraTransform.forward / 10 * MovementSpeed, NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                NPCPlayer.Position += Quaternion.AngleAxis(90, Vector3.up) * NPCPlayer.CameraTransform.forward / 10 * MovementSpeed;
                            }
                        }
                        catch (Exception) { }
                        break;

                    case MovementDirection.RIGHT:
                        try
                        {
                            if (!Physics.Linecast(NPCPlayer.Position, NPCPlayer.Position - Quaternion.AngleAxis(90, Vector3.up) * NPCPlayer.CameraTransform.forward / 10 * MovementSpeed, NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                NPCPlayer.Position -= Quaternion.AngleAxis(90, Vector3.up) * NPCPlayer.CameraTransform.forward / 10 * MovementSpeed;
                            }
                        }
                        catch (Exception) { }

                        break;

                    default:
                        break;
                }
                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        private IEnumerator<float> StartTalkCoroutine(Player p)
        {
            IsLocked = true;
            LockHandler = p;
            TalkingStates.Add(p, RootNode);
            bool end = RootNode.Send(Name, p);
            IsActionLocked = true;
            foreach (NodeAction action in RootNode.Actions.Keys)
            {
                action.Process(this, p, RootNode.Actions[action]);
                yield return Timing.WaitForSeconds(float.Parse(RootNode.Actions[action]["next_action_delay"].Replace('.', ',')));
            }
            IsActionLocked = false;
            if (end)
            {
                TalkingStates.Remove(p);
                p.SendConsoleMessage(Name + " ended talk", "yellow");
                IsLocked = false;
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
                            yield return Timing.WaitForSeconds(float.Parse(new_node.Actions[action]["next_action_delay"].Replace('.', ',')));
                        }
                        IsActionLocked = false;
                        if (end)
                        {
                            TalkingStates.Remove(p);
                            p.SendConsoleMessage(Name + " ended talk", "yellow");
                            IsLocked = false;
                        }
                    }
                    else
                    {
                        p.SendConsoleMessage("Invalid answer!", "red");
                    }
                }
                else
                {
                    p.SendConsoleMessage("Incorrect answer format!", "red");
                }
            }
            else
            {
                p.SendConsoleMessage("You aren't talking to this NPC!", "red");
            }
        }

        #endregion Coroutines

        #region Movement

        public void Move(MovementDirection dir)
        {
            CurMovementDirection = dir;
            switch (dir)
            {
                case MovementDirection.FORWARD:
                    NPCPlayer.ReferenceHub.animationController.Networkspeed = new Vector2(MovementSpeed, 0);
                    break;

                case MovementDirection.BACKWARD:
                    NPCPlayer.ReferenceHub.animationController.Networkspeed = new Vector2(-MovementSpeed, 0);
                    break;

                case MovementDirection.RIGHT:
                    NPCPlayer.ReferenceHub.animationController.Networkspeed = new Vector2(0, MovementSpeed);
                    break;

                case MovementDirection.LEFT:
                    NPCPlayer.ReferenceHub.animationController.Networkspeed = new Vector2(0, -MovementSpeed);
                    break;

                default:
                    NPCPlayer.ReferenceHub.animationController.Networkspeed = new Vector2(0, 0);
                    break;
            }
        }

        public void AddNavTarget(NavigationNode node)
        {
            NavigationQueue.Enqueue(node);
        }

        public void ClearNavTargets()
        {
            NavigationQueue.Clear();
        }

        public void Follow(Player p)
        {
            FollowTarget = p;
        }

        public float GoTo(Vector3 position)
        {
            IsActionLocked = true;
            Timing.KillCoroutines(MovementCoroutines);
            Vector3 heading = (position - NPCPlayer.Position);
            Quaternion lookRot = Quaternion.LookRotation(heading.normalized);
            float dist = heading.magnitude;
            NPCPlayer.Rotations = new Vector2(lookRot.eulerAngles.x, lookRot.eulerAngles.y);
            Move(MovementDirection.FORWARD);
            float eta = 0.1f * (dist / (NPCPlayer.CameraTransform.forward / 10 * MovementSpeed).magnitude);
            MovementCoroutines.Add(Timing.CallDelayed(eta, () =>
            {
                Move(MovementDirection.NONE);
                IsActionLocked = false;
            }));
            return eta;
        }

        #endregion Movement

        public void TalkWith(Player p)
        {
            AttachedCoroutines.Add(Timing.RunCoroutine(StartTalkCoroutine(p)));
        }

        public void HandleAnswer(Player p, string answer)
        {
            if (!IsActionLocked)
            {
                AttachedCoroutines.Add(Timing.RunCoroutine(HandleAnswerCoroutine(p, answer)));
            }
            else
            {
                p.SendConsoleMessage($"[{Name}] I'm busy now, wait a second", "yellow");
            }
        }

        public void Kill(bool spawn_ragdoll)
        {
            if (spawn_ragdoll)
            {
                gameObject.GetComponent<RagdollManager>().SpawnRagdoll(gameObject.transform.position, gameObject.transform.rotation, Vector3.zero, (int)NPCPlayer.Role, new PlayerStats.HitInfo(), false, "", Name, 9999);
            }
            UnityEngine.Object.Destroy(this);
        }

        public void FireEvent(NPCEvent ev)
        {
            try
            {
                ev.FireActions(Events[ev.Name]);
            }
            catch (KeyNotFoundException)
            {
                Log.Debug($"Skipping unused event {ev.Name}", Plugin.Instance.Config.VerboseOutput);
            }
        }

        private void OnDestroy()
        {
            Timing.KillCoroutines(MovementCoroutines);
            Timing.KillCoroutines(AttachedCoroutines);
            Log.Debug("Destroyed NPC component", Plugin.Instance.Config.VerboseOutput);
        }

        private void Awake()
        {
            NPCPlayer = Player.Get(gameObject);
            AttachedCoroutines.Add(Timing.RunCoroutine(UpdateTalking()));
            AttachedCoroutines.Add(Timing.RunCoroutine(MoveCoroutine()));
            AttachedCoroutines.Add(Timing.RunCoroutine(NavCoroutine()));
            Log.Debug($"Constructed NPC", Plugin.Instance.Config.DisplayNPCInPlayerList);
        }
    }
}