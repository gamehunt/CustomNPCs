using Exiled.API.Features;
using MEC;
using Mirror;
using NPCS.Talking;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace NPCS
{
    //This component provides interface to control NPC
    internal class Npc
    {
        //For simpler saving
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
                    return (int)parent.Role;
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
                    return Player.Get(parent.GameObject).IsGodModeEnabled;
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

        public GameObject GameObject { get; set; }

        public ReferenceHub ReferenceHub
        {
            get
            {
                return GameObject.GetComponent<ReferenceHub>();
            }
        }

        public NPCComponent NPCComponent
        {
            get
            {
                return GameObject.GetComponent<NPCComponent>();
            }
        }

        public TalkNode RootNode
        {
            get
            {
                return NPCComponent.__node;
            }
            set
            {
                NPCComponent.__node = value;
            }
        }

        public string Name
        {
            get
            {
                return GameObject.GetComponent<NicknameSync>().Network_myNickSync;
            }
            set
            {
                GameObject.GetComponent<NicknameSync>().Network_myNickSync = value;
            }
        }

        public Dictionary<Player, TalkNode> TalkingStates
        {
            get
            {
                return NPCComponent.TalkingStates;
            }
        }

        public RoleType Role
        {
            get
            {
                return GameObject.GetComponent<CharacterClassManager>().CurClass;
            }
            set
            {
                GameObject.GetComponent<CharacterClassManager>().CurClass = value;
            }
        }

        public ItemType ItemHeld
        {
            get
            {
                return GameObject.GetComponent<Inventory>().curItem;
            }
            set
            {
                GameObject.GetComponent<Inventory>().SetCurItem(value);
            }
        }

        public MovementDirection CurMovementDirection
        {
            get
            {
                return NPCComponent.curDir;
            }
            set
            {
                NPCComponent.curDir = value;
            }
        }

        public Vector3 Position
        {
            get
            {
                return GameObject.GetComponent<PlayerMovementSync>().RealModelPosition;
            }
            set
            {
                GameObject.GetComponent<PlayerMovementSync>().OverridePosition(value, 0f, false);
            }
        }

        public Npc(GameObject obj)
        {
            GameObject = obj;
        }

        private static IEnumerator<float> UpdateTalking(NPCComponent cmp)
        {
            for (; ; )
            {
                List<Player> invalid_players = new List<Player>();
                foreach (Player p in cmp.TalkingStates.Keys)
                {
                    if (!p.IsAlive || !Player.List.Contains(p) || Vector3.Distance(cmp.transform.position, p.Position) >= 3f)
                    {
                        invalid_players.Add(p);
                    }
                }
                foreach (Player p in invalid_players)
                {
                    cmp.TalkingStates.Remove(p);
                }
                yield return Timing.WaitForSeconds(0.5f);
            }
        }

        private static IEnumerator<float> MoveCoroutine(NPCComponent cmp)
        {
            for (; ; )
            {
                switch (cmp.curDir)
                {
                    case MovementDirection.FORWARD:
                        try
                        {
                            if (!Physics.Linecast(cmp.transform.position, cmp.transform.position + cmp.GetComponent<ReferenceHub>().PlayerCameraReference.forward / 10, cmp.GetComponent<PlayerMovementSync>().CollidableSurfaces))
                            {
                                cmp.GetComponent<PlayerMovementSync>().OverridePosition(cmp.transform.position + cmp.GetComponent<ReferenceHub>().PlayerCameraReference.forward / 10, cmp.transform.rotation.y, true);
                            }
                        }
                        catch (Exception e) { }
                        break;

                    case MovementDirection.BACKWARD:
                        try
                        {
                            if (!Physics.Linecast(cmp.transform.position, cmp.transform.position - cmp.GetComponent<ReferenceHub>().PlayerCameraReference.forward / 10, cmp.GetComponent<PlayerMovementSync>().CollidableSurfaces))
                            {
                                cmp.GetComponent<PlayerMovementSync>().OverridePosition(cmp.transform.position - cmp.GetComponent<ReferenceHub>().PlayerCameraReference.forward / 10, cmp.transform.rotation.y, true);
                            }
                        }
                        catch (Exception e) { }
                        break;

                    case MovementDirection.LEFT:
                        try
                        {
                            if (!Physics.Linecast(cmp.transform.position, cmp.transform.position + Quaternion.AngleAxis(90, Vector3.up) * cmp.GetComponent<ReferenceHub>().PlayerCameraReference.forward / 10, cmp.GetComponent<PlayerMovementSync>().CollidableSurfaces))
                            {
                                cmp.GetComponent<PlayerMovementSync>().OverridePosition(cmp.transform.position + Quaternion.AngleAxis(90, Vector3.up) * cmp.GetComponent<ReferenceHub>().PlayerCameraReference.forward / 10, cmp.transform.rotation.y, true);
                            }
                        }
                        catch (Exception e) { }
                        break;

                    case MovementDirection.RIGHT:
                        try
                        {
                            if (!Physics.Linecast(cmp.transform.position, cmp.transform.position - Quaternion.AngleAxis(90, Vector3.up) * cmp.GetComponent<ReferenceHub>().PlayerCameraReference.forward / 10, cmp.GetComponent<PlayerMovementSync>().CollidableSurfaces))
                            {
                                cmp.GetComponent<PlayerMovementSync>().OverridePosition(cmp.transform.position - Quaternion.AngleAxis(90, Vector3.up) * cmp.GetComponent<ReferenceHub>().PlayerCameraReference.forward / 10, cmp.transform.rotation.y, true);
                            }
                        }
                        catch (Exception e) { }

                        break;

                    default:
                        break;
                }

                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        public void Move(MovementDirection dir)
        {
            CurMovementDirection = dir;
            switch (dir)
            {
                case MovementDirection.FORWARD:
                    ReferenceHub.animationController.Networkspeed = new Vector2(1, 0);
                    break;

                case MovementDirection.BACKWARD:
                    ReferenceHub.animationController.Networkspeed = new Vector2(-1, 0);
                    break;

                case MovementDirection.RIGHT:
                    ReferenceHub.animationController.Networkspeed = new Vector2(0, 1);
                    break;

                case MovementDirection.LEFT:
                    ReferenceHub.animationController.Networkspeed = new Vector2(0, -1);
                    break;

                default:
                    ReferenceHub.animationController.Networkspeed = new Vector2(0, 0);
                    break;
            }
        }

        public void TalkWith(Player p)
        {
            TalkingStates.Add(p, RootNode);
            bool end = RootNode.Send(Name, p);
            foreach (NodeAction action in RootNode.Actions.Keys)
            {
                action.Process(this, p, RootNode.Actions[action]);
            }
            if (end)
            {
                TalkingStates.Remove(p);
                p.SendConsoleMessage(Name + " ended talk", "yellow");
            }
        }

        public void HandleAnswer(Player p, string answer)
        {
            if (TalkingStates.ContainsKey(p))
            {
                TalkNode cur_node = TalkingStates[p];
                if (int.TryParse(answer, out int node))
                {
                    if (cur_node.NextNodes.TryGet(node, out TalkNode new_node))
                    {
                        TalkingStates[p] = new_node;
                        bool end = new_node.Send(Name, p);
                        foreach (NodeAction action in new_node.Actions.Keys)
                        {
                            action.Process(this, p, new_node.Actions[action]);
                        }
                        if (end)
                        {
                            TalkingStates.Remove(p);
                            p.SendConsoleMessage(Name + " ended talk", "yellow");
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

        public static Npc CreateNPC(Vector3 pos, Quaternion rot, RoleType type = RoleType.ClassD, ItemType itemHeld = ItemType.None, string name = "(EMPTY)", string root_node = "default_node.yml")
        {
            GameObject obj =
                UnityEngine.Object.Instantiate(
                    NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
            CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();

            obj.transform.localScale = Vector3.one;
            obj.transform.position = pos;
            obj.transform.rotation = rot;

            obj.GetComponent<QueryProcessor>().PlayerId = QueryProcessor._idIterator++;
            obj.GetComponent<QueryProcessor>()._ipAddress = "127.0.0.WAN";
            ccm._privUserId = $"{name}-{obj.GetComponent<QueryProcessor>().PlayerId }@NPC";

            ccm.SetClassID(type);
            ccm.CurClass = type;

            obj.GetComponent<NicknameSync>().Network_myNickSync = name;
            obj.GetComponent<ServerRoles>().MyText = "NPC";
            obj.GetComponent<ServerRoles>().MyColor = "red";

            NPCComponent npcc = obj.AddComponent<NPCComponent>();

            NetworkServer.Spawn(obj);
            PlayerManager.AddPlayer(obj);

            Player ply_obj = new Player(obj);
            Player.Dictionary.Add(obj, ply_obj);

            Player.IdsCache.Add(ply_obj.Id, ply_obj);
            Player.UserIdsCache.Add(ccm._privUserId, ply_obj);

            Npc b = new Npc(obj)
            {
                RootNode = (TalkNode.FromFile(Path.Combine(Config.NPCs_nodes_path, root_node))),
                ItemHeld = (itemHeld)
            };
            npcc.talking_coroutine = Timing.RunCoroutine(UpdateTalking(npcc));
            npcc.movement_coroutine = Timing.RunCoroutine(MoveCoroutine(npcc));
            Timing.CallDelayed(0.3f, () => b.ReferenceHub.playerMovementSync.OverridePosition(pos, rot.y, true));
            return b;
        }

        //NPC format
        //name: SomeName
        //role: 6
        //item_held: 24
        //root_node: default_node.yml
        //god_mode: false
        public static Npc CreateNPC(Vector3 pos, Quaternion rot, string path)
        {
            try
            {
                var input = new StringReader(File.ReadAllText(Path.Combine(Config.NPCs_root_path, path)));

                var yaml = new YamlStream();
                yaml.Load(input);

                var mapping =
                    (YamlMappingNode)yaml.Documents[0].RootNode;

                Npc n = CreateNPC(pos, rot, (RoleType)int.Parse((string)mapping.Children[new YamlScalarNode("role")]), (ItemType)int.Parse((string)mapping.Children[new YamlScalarNode("item_held")]), (string)mapping.Children[new YamlScalarNode("name")], (string)mapping.Children[new YamlScalarNode("root_node")]);
                if (bool.Parse((string)mapping.Children[new YamlScalarNode("god_mode")]))
                {
                    Log.Info("Switched GOD_MODE");
                    Player pl = Player.Get(n.GameObject);
                    pl.IsGodModeEnabled = true;
                }
                return n;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to load NPC from {path}: {e}");
                return null;
            }
        }

        public static Npc FromComponent(NPCComponent npc)
        {
            return new Npc(npc.gameObject);
        }

        public void Kill(bool spawn_ragdoll)
        {
            if (spawn_ragdoll)
            {
                GameObject.GetComponent<RagdollManager>().SpawnRagdoll(GameObject.transform.position, GameObject.transform.rotation, Vector3.zero, (int)ReferenceHub.characterClassManager.CurClass, new PlayerStats.HitInfo(), false, "", Name, 9999);
            }
            UnityEngine.Object.Destroy(GameObject);
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
    }
}