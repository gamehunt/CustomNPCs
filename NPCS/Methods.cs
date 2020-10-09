using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using Mirror;
using NPCS.AI;
using NPCS.Events;
using NPCS.Navigation;
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
    public class Methods
    {
        public static Npc CreateNPC(Vector3 pos, Vector2 rot, Vector3 scale, RoleType type = RoleType.ClassD, ItemType itemHeld = ItemType.None, string name = "(EMPTY)", string root_node = "default_node.yml")
        {
            GameObject obj =
                UnityEngine.Object.Instantiate(
                    NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
            CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();

            pos = new Vector3(pos.x, pos.y - (1f - scale.y) * Plugin.Instance.Config.NpcSizePositionMultiplier, pos.z);

            obj.transform.localScale = scale;
            obj.transform.position = pos;

            QueryProcessor processor = obj.GetComponent<QueryProcessor>();

            processor.NetworkPlayerId = QueryProcessor._idIterator++;
            processor._ipAddress = "127.0.0.WAN";

            ccm.CurClass = type;
            obj.GetComponent<PlayerStats>().SetHPAmount(ccm.Classes.SafeGet(type).maxHP);

            obj.GetComponent<NicknameSync>().Network_myNickSync = name;

            ServerRoles roles = obj.GetComponent<ServerRoles>();

            roles.MyText = "NPC";
            roles.MyColor = "red";

            NetworkServer.Spawn(obj);
            PlayerManager.AddPlayer(obj); //I'm not sure if I need this

            Player ply_obj = new Player(obj);
            Player.Dictionary.Add(obj, ply_obj);

            Player.IdsCache.Add(ply_obj.Id, ply_obj);

            Npc npcc = obj.AddComponent<Npc>();

            npcc.MovementSpeed = CharacterClassManager._staticClasses[(int)type].walkSpeed;

            npcc.ItemHeld = itemHeld;
            npcc.RootNode = TalkNode.FromFile(Path.Combine(Config.NPCs_nodes_path, root_node));

            npcc.NPCPlayer.ReferenceHub.transform.localScale = scale;

            npcc.AttachedCoroutines.Add(Timing.CallDelayed(0.3f, () =>
            {
                npcc.NPCPlayer.ReferenceHub.playerMovementSync.OverridePosition(pos, 0, true);
                npcc.NPCPlayer.Rotations = rot;
            }));

            npcc.AttachedCoroutines.Add(Timing.CallDelayed(0.4f, () =>
            {
                npcc.FireEvent(new NPCOnCreatedEvent(npcc, null));
            }));

            return npcc;
        }

        //NPC format
        //name: SomeName
        //health: -1
        //role: Scientist
        //scale: [1, 1, 1]
        //item_held: GunLogicer
        //root_node: default_node.yml
        //god_mode: false
        //is_exclusive: true
        //events: []
        //ai_enabled: false
        //ai: []

        //TODO use deserializers, this shit is really stupid
        public static Npc CreateNPC(Vector3 pos, Vector2 rot, string path)
        {
            try
            {
                var input = new StringReader(File.ReadAllText(Path.Combine(Config.NPCs_root_path, path)));

                var yaml = new YamlStream();
                yaml.Load(input);

                var mapping =
                    (YamlMappingNode)yaml.Documents[0].RootNode;

                YamlSequenceNode scale = (YamlSequenceNode)mapping.Children[new YamlScalarNode("scale")];
                float x = float.Parse(((string)scale.Children[0]).Replace('.', ','));
                float y = float.Parse(((string)scale.Children[1]).Replace('.', ','));
                float z = float.Parse(((string)scale.Children[2]).Replace('.', ','));

                Npc n = CreateNPC(pos, rot, new Vector3(x, y, z), (RoleType)Enum.Parse(typeof(RoleType), (string)mapping.Children[new YamlScalarNode("role")]), (ItemType)Enum.Parse(typeof(ItemType), (string)mapping.Children[new YamlScalarNode("item_held")]), (string)mapping.Children[new YamlScalarNode("name")], (string)mapping.Children[new YamlScalarNode("root_node")]);

                n.NPCPlayer.IsGodModeEnabled = bool.Parse((string)mapping.Children[new YamlScalarNode("god_mode")]);

                n.IsExclusive = bool.Parse((string)mapping.Children[new YamlScalarNode("is_exclusive")]);

                int health = int.Parse((string)mapping.Children[new YamlScalarNode("health")]);

                n.SaveFile = path;
                if (health > 0)
                {
                    n.NPCPlayer.MaxHealth = health;
                    n.NPCPlayer.Health = health;
                }

                Log.Info("Parsing events...");

                YamlSequenceNode events = (YamlSequenceNode)mapping.Children[new YamlScalarNode("events")];

                foreach (YamlMappingNode event_node in events.Children)
                {
                    var actions = (YamlSequenceNode)event_node.Children[new YamlScalarNode("actions")];
                    Dictionary<NodeAction, Dictionary<string, string>> actions_mapping = new Dictionary<NodeAction, Dictionary<string, string>>();
                    foreach (YamlMappingNode action_node in actions)
                    {
                        NodeAction act = NodeAction.GetFromToken((string)action_node.Children[new YamlScalarNode("token")]);
                        if (act != null)
                        {
                            Log.Debug($"Recognized action: {act.Name}", Plugin.Instance.Config.VerboseOutput);
                            var yml_args = (YamlMappingNode)action_node.Children[new YamlScalarNode("args")];
                            Dictionary<string, string> arg_bindings = new Dictionary<string, string>();
                            foreach (YamlScalarNode arg in yml_args.Children.Keys)
                            {
                                arg_bindings.Add((string)arg.Value, (string)yml_args.Children[arg]);
                            }
                            actions_mapping.Add(act, arg_bindings);
                        }
                        else
                        {
                            Log.Error($"Failed to parse action: {(string)action_node.Children[new YamlScalarNode("token")]} (invalid token)");
                        }
                    }
                    n.Events.Add((string)event_node.Children[new YamlScalarNode("token")], actions_mapping);
                }

                n.AIEnabled = bool.Parse((string)mapping.Children[new YamlScalarNode("ai_enabled")]);

                YamlSequenceNode ai_targets = (YamlSequenceNode)mapping.Children[new YamlScalarNode("ai")];

                foreach (YamlMappingNode ai_node in ai_targets.Children)
                {
                    AI.AITarget act = AITarget.GetFromToken((string)ai_node.Children[new YamlScalarNode("token")]);
                    if (act != null)
                    {
                        Log.Debug($"Recognized ai target: {act.Name}", Plugin.Instance.Config.VerboseOutput);
                        var yml_args = (YamlMappingNode)ai_node.Children[new YamlScalarNode("args")];
                        Dictionary<string, string> arg_bindings = new Dictionary<string, string>();
                        foreach (YamlScalarNode arg in yml_args.Children.Keys)
                        {
                            act.Arguments.Add((string)arg.Value, (string)yml_args.Children[arg]);
                        }
                        n.AIQueue.AddLast(act);
                    }
                    else
                    {
                        Log.Error($"Failed to parse ai node: {(string)ai_node.Children[new YamlScalarNode("token")]} (invalid token)");
                    }
                }
                return n;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to load NPC from {path}: {e}");
                return null;
            }
        }

        //Oh... More shitcode!
        public static void GenerateNavGraph()
        {
            Log.Info("[NAV] Generating navigation graph...");
            StreamReader sr = File.OpenText(Config.NPCs_nav_mappings_path);
            var deserializer = new DeserializerBuilder().Build();
            Dictionary<string, List<NavigationNode.NavNodeSerializationInfo>> manual_mappings = deserializer.Deserialize<Dictionary<string, List<NavigationNode.NavNodeSerializationInfo>>>(sr);
            sr.Close();
            foreach (Room r in Map.Rooms)
            {
                string rname = r.Name.RemoveBracketsOnEndOfName();
                if (!manual_mappings.ContainsKey(rname))
                {
                    NavigationNode node = NavigationNode.Create(r.Position, $"AUTO_Room_{r.Name}".Replace(' ', '_'));
                    foreach (Door d in r.Doors)
                    {
                        if (d.gameObject.transform.position == Vector3.zero)
                        {
                            continue;
                        }
                        NavigationNode new_node = NavigationNode.Create(d.gameObject.transform.position, $"AUTO_Door_{(d.DoorName.IsEmpty() ? d.gameObject.transform.position.ToString() : d.DoorName)}".Replace(' ', '_'));
                        if (new_node == null)
                        {
                            new_node = NavigationNode.AllNodes[$"AUTO_Door_{(d.DoorName.IsEmpty() ? d.gameObject.transform.position.ToString() : d.DoorName)}".Replace(' ', '_')];
                        }
                        else
                        {
                            new_node.AttachedDoor = d;
                        }
                        node.LinkedNodes.Add(new_node);
                        new_node.LinkedNodes.Add(node);
                    }
                }
                else
                {
                    Log.Debug($"Loading manual mappings for room {r.Name}");
                    List<NavigationNode.NavNodeSerializationInfo> nodes = manual_mappings[rname];
                    int i = 0;
                    foreach (Door d in r.Doors)
                    {
                        if (d.gameObject.transform.position == Vector3.zero)
                        {
                            continue;
                        }
                        NavigationNode new_node = NavigationNode.Create(d.gameObject.transform.position, $"AUTO_Door_{(d.DoorName.IsEmpty() ? d.gameObject.transform.position.ToString() : d.DoorName)}".Replace(' ', '_'));
                        if (new_node != null)
                        {
                            new_node.AttachedDoor = d;
                        }
                        else
                        {
                            new_node = NavigationNode.AllNodes[$"AUTO_Door_{(d.DoorName.IsEmpty() ? d.gameObject.transform.position.ToString() : d.DoorName)}".Replace(' ', '_')];
                        }
                    }
                    foreach (NavigationNode.NavNodeSerializationInfo info in nodes)
                    {
                        NavigationNode node = NavigationNode.Create(info, $"MANUAL_Room_{r.Name}_{i}", rname);
                        foreach (NavigationNode d in NavigationNode.AllNodes.Values.Where(nd => nd != node && Vector3.Distance(nd.Position, node.Position) < 3f))
                        {
                            node.LinkedNodes.Add(d);
                            d.LinkedNodes.Add(node);

                            Log.Info($"Linked {node.Name} and {d.Name}");
                        }
                        i++;
                    }
                }
            }
        }
    }
}