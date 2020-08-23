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
        private TalkNode root_node;
        private Dictionary<Player, TalkNode> talking_states = new Dictionary<Player, TalkNode>();

        private List<CoroutineHandle> attached_coroutines = new List<CoroutineHandle>();
        private List<CoroutineHandle> movement_coroutines = new List<CoroutineHandle>();

        private Dictionary<string, Dictionary<NodeAction, Dictionary<string, string>>> attached_events = new Dictionary<string, Dictionary<NodeAction, Dictionary<string, string>>>(); //Horrible

        private Queue<NavigationNode> nav_queue = new Queue<NavigationNode>();
        private NavigationNode nav_current_target = null;

        private Player follow_target = null;

        private MovementDirection curDir;

        private bool action_locked = false;
        private Player lock_handler = null;
        private bool locked = false;

        private bool is_exclusive = false;

        private float speed = 2f;

        private Player player;

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
                    return (int)parent.player.Role;
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

        public enum MovementDirection
        {
            NONE,
            FORWARD,
            BACKWARD,
            LEFT,
            RIGHT
        };

        public Player NPCPlayer
        {
            get
            {
                return player;
            }
            set
            {
                player = value;
            }
        }

        public TalkNode RootNode
        {
            get
            {
                return root_node;
            }
            set
            {
                root_node = value;
            }
        }

        public string Name
        {
            get
            {
                return player.ReferenceHub.nicknameSync.Network_myNickSync;
            }
            set
            {
                player.ReferenceHub.nicknameSync.Network_myNickSync = value;
            }
        }

        public Dictionary<Player, TalkNode> TalkingStates
        {
            get
            {
                return talking_states;
            }
        }

        public ItemType ItemHeld
        {
            get
            {
                return player.ReferenceHub.inventory.curItem;
            }
            set
            {
                player.ReferenceHub.inventory.SetCurItem(value);
            }
        }

        public MovementDirection CurMovementDirection
        {
            get
            {
                return curDir;
            }
            set
            {
                curDir = value;
            }
        }

        public bool IsLocked
        {
            get
            {
                return locked;
            }
            set
            {
                locked = value;
            }
        }

        public Player LockHandler
        {
            get
            {
                return lock_handler;
            }
            set
            {
                lock_handler = value;
            }
        }

        public bool IsActionLocked
        {
            get
            {
                return action_locked;
            }
            set
            {
                action_locked = value;
            }
        }

        public bool IsExclusive
        {
            get
            {
                return is_exclusive;
            }
            set
            {
                is_exclusive = value;
            }
        }

        public Dictionary<string, Dictionary<NodeAction, Dictionary<string, string>>> Events
        {
            get
            {
                return attached_events;
            }
        }

        public Queue<NavigationNode> NavigationQueue
        {
            get
            {
                return nav_queue;
            }
        }

        public NavigationNode CurrentNavTarget
        {
            get
            {
                return nav_current_target;
            }
            set
            {
                nav_current_target = value;
            }
        }

        public Player FollowTarget
        {
            get
            {
                return follow_target;
            }
            set
            {
                follow_target = value;
            }
        }

        public float MovementSpeed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }

        public List<CoroutineHandle> AttachedCoroutines
        {
            get
            {
                return attached_coroutines;
            }
        }

        public List<CoroutineHandle> MovementCoroutines
        {
            get
            {
                return movement_coroutines;
            }
        }

        //------------------------------------------ Coroutines

        private IEnumerator<float> NavCoroutine()
        {
            for (; ; )
            {
                if (FollowTarget != null)
                {
                    if (FollowTarget.IsAlive)
                    {
                        GoTo(FollowTarget.Position);
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
                foreach (Player p in talking_states.Keys)
                {
                    if (!p.IsAlive || !Player.List.Contains(p))
                    {
                        invalid_players.Add(p);
                    }
                }
                foreach (Player p in invalid_players)
                {
                    talking_states.Remove(p);
                    if (p == lock_handler)
                    {
                        lock_handler = null;
                        locked = false;
                    }
                }
                yield return Timing.WaitForSeconds(0.5f);
            }
        }

        private IEnumerator<float> MoveCoroutine()
        {
            for (; ; )
            {
                switch (curDir)
                {
                    case MovementDirection.FORWARD:
                        try
                        {
                            if (!Physics.Linecast(player.Position, player.Position + player.CameraTransform.forward / 10 * speed, player.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                player.Position += player.CameraTransform.forward / 10 * speed;
                            }
                        }
                        catch (Exception) { }
                        break;

                    case MovementDirection.BACKWARD:
                        try
                        {
                            if (!Physics.Linecast(player.Position, gameObject.transform.position - player.CameraTransform.forward / 10 * speed, player.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                player.Position -= player.CameraTransform.forward / 10 * speed;
                            }
                        }
                        catch (Exception) { }
                        break;

                    case MovementDirection.LEFT:
                        try
                        {
                            if (!Physics.Linecast(player.Position, player.Position + Quaternion.AngleAxis(90, Vector3.up) * player.CameraTransform.forward / 10 * speed, player.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                player.Position +=  Quaternion.AngleAxis(90, Vector3.up) * player.CameraTransform.forward / 10 * speed;
                            }
                        }
                        catch (Exception) { }
                        break;

                    case MovementDirection.RIGHT:
                        try
                        {
                            if (!Physics.Linecast(player.Position, player.Position - Quaternion.AngleAxis(90, Vector3.up) * player.CameraTransform.forward / 10 * speed, player.ReferenceHub.playerMovementSync.CollidableSurfaces))
                            {
                                player.Position -= Quaternion.AngleAxis(90, Vector3.up) * player.CameraTransform.forward / 10 * speed;
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

        //------------------------------------------

        public void Move(MovementDirection dir)
        {
            CurMovementDirection = dir;
            switch (dir)
            {
                case MovementDirection.FORWARD:
                    player.ReferenceHub.animationController.Networkspeed = new Vector2(MovementSpeed, 0);
                    break;

                case MovementDirection.BACKWARD:
                    player.ReferenceHub.animationController.Networkspeed = new Vector2(-MovementSpeed, 0);
                    break;

                case MovementDirection.RIGHT:
                    player.ReferenceHub.animationController.Networkspeed = new Vector2(0, MovementSpeed);
                    break;

                case MovementDirection.LEFT:
                    player.ReferenceHub.animationController.Networkspeed = new Vector2(0, -MovementSpeed);
                    break;

                default:
                    player.ReferenceHub.animationController.Networkspeed = new Vector2(0, 0);
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
            Timing.KillCoroutines(movement_coroutines);
            Vector3 heading = (position - player.Position);
            Quaternion lookRot = Quaternion.LookRotation(heading.normalized);
            float dist = heading.magnitude;
            player.Rotations = new Vector2(lookRot.eulerAngles.x, lookRot.eulerAngles.y);
            Move(MovementDirection.FORWARD);
            float eta = 0.1f * (dist / (player.CameraTransform.forward / 10 * MovementSpeed).magnitude);
            movement_coroutines.Add(Timing.CallDelayed(eta, () =>
            {
                Move(MovementDirection.NONE);
                IsActionLocked = false;
            }));
            return eta;
        }

        public void TalkWith(Player p)
        {
            attached_coroutines.Add(Timing.RunCoroutine(StartTalkCoroutine(p)));
        }

        public void HandleAnswer(Player p, string answer)
        {
            if (!IsActionLocked)
            {
                attached_coroutines.Add(Timing.RunCoroutine(HandleAnswerCoroutine(p, answer)));
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
                gameObject.GetComponent<RagdollManager>().SpawnRagdoll(gameObject.transform.position, gameObject.transform.rotation, Vector3.zero, (int)player.Role, new PlayerStats.HitInfo(), false, "", Name, 9999);
            }
            UnityEngine.Object.Destroy(this);
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
            Log.Debug("Destroying NPC component", Plugin.Instance.Config.VerboseOutput);
            Timing.KillCoroutines(movement_coroutines);
            Timing.KillCoroutines(attached_coroutines);
        }

        private void Awake()
        {
            player = Player.Get(gameObject);
            attached_coroutines.Add(Timing.RunCoroutine(UpdateTalking()));
            attached_coroutines.Add(Timing.RunCoroutine(MoveCoroutine()));
            attached_coroutines.Add(Timing.RunCoroutine(NavCoroutine()));
            Log.Debug($"Constructed NPC",Plugin.Instance.Config.DisplayNPCInPlayerList);
        }
    }
}