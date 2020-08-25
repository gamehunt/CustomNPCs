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

        public static List<Npc> List { get; } = new List<Npc>();

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
                return NPCPlayer.Inventory.curItem;
            }
            set
            {
                NPCPlayer.Inventory.curItem = value;
            }
        }

        public MovementDirection CurMovementDirection { get; set; }

        public bool IsLocked { get; set; } = false;

        public Player LockHandler { get; set; } = null;

        public bool IsActionLocked { get; set; } = false;

        public bool IsExclusive { get; set; } = false;

        public Dictionary<string, Dictionary<NodeAction, Dictionary<string, string>>> Events { get; } = new Dictionary<string, Dictionary<NodeAction, Dictionary<string, string>>>();

        public Queue<NavigationNode> NavigationQueue { get; private set; } = new Queue<NavigationNode>();

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
                        if (Vector3.Distance(FollowTarget.Position, NPCPlayer.Position) >= 15f)
                        {
                            NPCPlayer.Position = FollowTarget.Position;
                        }
                    }
                    else
                    {
                        FireEvent(new NPCFollowTargetDiedEvent(this, FollowTarget));
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
            if (TalkingStates.ContainsKey(p))
            {
                p.SendConsoleMessage($"[{Name}] We are already talking!", "yellow");
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

        #region Navigation

        private bool TryProcessNode(NavigationNode target, NavigationNode current, ref Stack<NavigationNode> queue, ref HashSet<NavigationNode> visited_nodes)
        {
            queue.Push(current);
            visited_nodes.Add(current);
            //Log.Debug($"Way builder: in node {current.Name}");
            foreach (NavigationNode node in current.LinkedNodes)
            {
                if (node == target)
                {
                    queue.Push(node);
                    return true;
                }
                if(visited_nodes.Contains(node))
                {
                    continue;
                }
                else if (node.LinkedNodes.Count > 0)
                {
                    if (TryProcessNode(target, node, ref queue, ref visited_nodes))
                    {
                        return true;
                    }
                }
                else
                {
                    Log.Debug($"No links on node: {node.Name}");
                }
            }
            queue.Pop();
            return false;
        }

        public void GotoRoom(Room r)
        {
            NavigationNode target_node = NavigationNode.FromRoom(r);
            if (target_node != null)
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
                    Log.Info($"Selected nearest node: {nearest_node.Name}");
                    if (nearest_node == target_node)
                    {
                        GoTo(target_node.Position);
                    }
                    else
                    {
                        Stack<NavigationNode> new_nav_queue = new Stack<NavigationNode>();
                        HashSet<NavigationNode> visited = new HashSet<NavigationNode>();
                        if (!TryProcessNode(target_node, nearest_node, ref new_nav_queue, ref visited))
                        {
                            Log.Error("[NAV] Failed to build way");
                        }
                        else
                        {
                            Log.Debug("Built way:",Plugin.Instance.Config.VerboseOutput);
                            NavigationQueue.Clear();
                            List<NavigationNode> reversed_stack = new_nav_queue.Reverse().ToList();
                            foreach(NavigationNode node in reversed_stack){
                                Log.Debug(node.Name, Plugin.Instance.Config.VerboseOutput);
                                NavigationQueue.Enqueue(node);
                            }
                        }
                    }
                }
                else
                {
                    Log.Error("[NAV] Failed to find nearest navnode");
                }
            }
            else
            {
                Log.Error("[NAV] Specified room has null navnode");
            }
        }

        #endregion Navigation

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
            UnityEngine.Object.Destroy(NPCPlayer.GameObject);
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
            List.Remove(this);
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
            List.Add(this);
            Log.Debug($"Constructed NPC", Plugin.Instance.Config.VerboseOutput);
        }
    }
}