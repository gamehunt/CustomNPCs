using Exiled.API.Extensions;
using Exiled.API.Features;
using Interactables.Interobjects.DoorUtils;
using MEC;
using Mirror;
using NPCS.AI;
using NPCS.Events;
using NPCS.Navigation;
using NPCS.Talking;
using NPCS.Utils;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeInspectors;

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

            Npc npcc = obj.AddComponent<Npc>();
            npcc.RootNode = TalkNode.FromFile(Path.Combine(Config.NPCs_nodes_path, root_node));

            Timing.CallDelayed(0.1f, () =>
            {
                    Player.UnverifiedPlayers.TryGetValue(ccm._hub, out Player ply_obj);
                    Player.Dictionary.Add(obj, ply_obj);
                    ply_obj.IsVerified = true;

                    npcc.FinishInitialization();

                    npcc.ItemHeld = itemHeld;
                    npcc.NPCPlayer.ReferenceHub.transform.localScale = scale;
                    npcc.NPCPlayer.SessionVariables.Add("IsNPC", true);
            });

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
        //inventory: [KeycardO5, GunLogicer]
        //item_held: GunLogicer
        //root_node: default_node.yml
        //god_mode: false
        //is_exclusive: true
        //process_scp_logic: false
        //affect_summary: false
        //events: []
        //ai_enabled: false
        //ai_mode: Legacy
        //ai: []
        //ai_scripts: []

        public static Npc CreateNPC(Vector3 pos, Vector2 rot, string path)
        {
            try
            {
                var input = new StringReader(File.ReadAllText(Path.Combine(Config.NPCs_root_path, path)));

                var deserializer = new DeserializerBuilder()
                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                    // Workaround to remove YamlAttributesTypeInspector
                                    .WithTypeInspector(inner => inner, s => s.InsteadOf<YamlAttributesTypeInspector>())
                                    .WithTypeInspector(
                                        inner => new YamlAttributesTypeInspector(inner),
                                        s => s.Before<NamingConventionTypeInspector>()
                                    )
                                    .Build();

                NpcSerializationInfo raw_npc = deserializer.Deserialize<NpcSerializationInfo>(input);

                Npc n = CreateNPC(pos, rot, new Vector3(raw_npc.Scale[0], raw_npc.Scale[1], raw_npc.Scale[2]), raw_npc.Role, ItemType.None, raw_npc.Name, raw_npc.RootNode);

                Timing.CallDelayed(0.2f, () =>
                {
                    foreach (ItemType type in raw_npc.Inventory)
                    {
                        Log.Debug($"Added item: {type:g}");
                        n.TakeItem(type);
                    }

                    n.ItemHeld = raw_npc.ItemHeld;
                    n.NPCPlayer.IsGodModeEnabled = raw_npc.GodMode;

                    int health = raw_npc.Health;

                    if (health > 0)
                    {
                        n.NPCPlayer.MaxHealth = health;
                        n.NPCPlayer.Health = health;
                    }
                });

                n.IsExclusive = raw_npc.IsExclusive;
                n.SaveFile = path;
                n.AffectRoundSummary = raw_npc.AffectSummary;
                n.ProcessSCPLogic = raw_npc.ProcessScpLogic;

                n.AIMode = raw_npc.AiMode;

                foreach (string scr in raw_npc.AiScripts)
                {
                    string full = Path.Combine(Config.NPCs_scripts_path, scr);
                    Log.Debug($"Enabling script: {full}", Plugin.Instance.Config.VerboseOutput);
                    n.AIScripts.Add(full);
                }

                Log.Debug($"Set AI mode: {n.AIMode}", Plugin.Instance.Config.VerboseOutput);
                if (n.AIMode == AIMode.Legacy)
                {
                    Log.Warn("YML AI has been deprecated and will be removed soon! Please, switch to Python - it much more flexible and simplier to code.");
                }

                Log.Info("Parsing events...");

                foreach (NpcEventSerializationInfo info in raw_npc.Events)
                {
                    List<KeyValuePair<NodeAction, Dictionary<string, string>>> actions_mapping = new List<KeyValuePair<NodeAction, Dictionary<string, string>>>();
                    foreach (NpcNodeWithArgsSerializationInfo action in info.Actions)
                    {
                        NodeAction act = NodeAction.GetFromToken(action.Token);
                        if (act != null)
                        {
                            actions_mapping.Add(new KeyValuePair<NodeAction, Dictionary<string, string>>(act, action.Args));
                        }
                        else
                        {
                            Log.Error($"Failed to event action: {info.Token} (invalid token)");
                        }
                    }
                    n.Events.Add(info.Token, actions_mapping);
                }

                n.AIEnabled = raw_npc.AiEnabled;

                n.StartAI();

                foreach (NpcNodeWithArgsSerializationInfo info in raw_npc.Ai)
                {
                    AI.AITarget act = AITarget.GetFromToken(info.Token);
                    if (act != null)
                    {
                        Log.Debug($"Recognized ai target: {act.Name}", Plugin.Instance.Config.VerboseOutput);
                        act.Arguments = info.Args;
                        if (act.Verified)
                        {
                            n.AIQueue.AddLast(act);
                        }
                        else
                        {
                            Log.Warn($"Failed to verify config or construct {act.Name}, it will be skipped!");
                        }
                    }
                    else
                    {
                        Log.Error($"Failed to parse ai node: {info.Token} (invalid token)");
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
            try
            {
                Log.Info("[NAV] Generating navigation graph...");

                StreamReader sr = File.OpenText(Config.NPCs_nav_mappings_path);
                var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                Dictionary<string, List<NavigationNode.NavNodeSerializationInfo>> manual_mappings = deserializer.Deserialize<Dictionary<string, List<NavigationNode.NavNodeSerializationInfo>>>(sr);
                sr.Close();

                Log.Info("[NAV] Mapping lifts...");
                foreach (Lift lift in Map.Lifts)
                {
                    int i = 0;
                    NavigationNode prev_node = null;
                    foreach (Lift.Elevator elevator in lift.elevators)
                    {
                        NavigationNode node = NavigationNode.Create(elevator.target.position, $"AUTO_Elevator_{lift.elevatorName}_{i}".Replace(' ', '_'));
                        node.AttachedElevator = new KeyValuePair<Lift.Elevator, Lift>(elevator, lift);
                        i++;
                        if (prev_node != null)
                        {
                            prev_node.LinkedNodes.Add(node);
                            node.LinkedNodes.Add(prev_node);
                        }
                        else
                        {
                            prev_node = node;
                        }
                    }
                }
                foreach (Room r in Map.Rooms)
                {
                    string rname = r.Name.RemoveBracketsOnEndOfName();
                    if (!manual_mappings.ContainsKey(rname))
                    {
                        NavigationNode node = NavigationNode.Create(r.Position, $"AUTO_Room_{r.Name}".Replace(' ', '_'));
                        foreach (DoorVariant d in r.Doors)
                        {
                            if (d == null)
                            {
                                continue;
                            }
                            if (d.gameObject.transform.position == Vector3.zero)
                            {
                                continue;
                            }
                            NavigationNode new_node = NavigationNode.Create(d.gameObject.transform.position, $"AUTO_Door_{(d.gameObject.transform.position)}".Replace(' ', '_'));
                            if (new_node == null)
                            {
                                new_node = NavigationNode.AllNodes[$"AUTO_Door_{(d.gameObject.transform.position)}".Replace(' ', '_')];
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
                        bool is_first = true;
                        Log.Debug($"Loading manual mappings for room {r.Name}", Plugin.Instance.Config.VerboseOutput);
                        List<NavigationNode.NavNodeSerializationInfo> nodes = manual_mappings[rname];
                        int i = 0;
                        foreach (DoorVariant d in r.Doors)
                        {
                            if (d == null) //idk
                            {
                                continue;
                            }
                            if (d.gameObject.transform.position == Vector3.zero)
                            {
                                continue;
                            }
                            NavigationNode new_node = NavigationNode.Create(d.gameObject.transform.position, $"AUTO_Door_{(d.gameObject.transform.position)}".Replace(' ', '_'));
                            if (new_node != null)
                            {
                                new_node.AttachedDoor = d;
                            }
                            else
                            {
                                new_node = NavigationNode.AllNodes[$"AUTO_Door_{(d.gameObject.transform.position)}".Replace(' ', '_')];
                            }
                        }
                        foreach (NavigationNode.NavNodeSerializationInfo info in nodes)
                        {
                            NavigationNode node = NavigationNode.Create(info, is_first ? $"AUTO_Room_{r.Name}".Replace(' ', '_') : $"MANUAL_Room_{r.Name}_{i}".Replace(' ', '_'), rname);
                            is_first = false;
                            foreach (NavigationNode d in NavigationNode.AllNodes.Values.Where(nd => nd != node && Vector3.Distance(nd.Position, node.Position) < Plugin.Instance.Config.NavNodeMapperMaxDistance))
                            {
                                node.LinkedNodes.Add(d);
                                d.LinkedNodes.Add(node);

                                Log.Debug($"Linked {node.Name} and {d.Name}", Plugin.Instance.Config.VerboseOutput);
                            }
                            i++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Caught an exception while generating navigation graph: {e}/{e.StackTrace}");
            }
        }

        public static void SaveNPCMappings(string path)
        {
            path = Path.Combine(Config.NPCs_mappings_path, path);
            StreamWriter sw;
            if (!File.Exists(path))
            {
                sw = File.CreateText(path);
                var serializer = new SerializerBuilder().Build();
                List<Npc.NPCMappingInfo> infos = new List<Npc.NPCMappingInfo>();
                foreach (Npc n in Npc.List)
                {
                    if (n.SaveFile != null)
                    {
                        infos.Add(new Npc.NPCMappingInfo(n));
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

        private static IEnumerator<float> NPCMappingsLoadCoroutine(List<Npc.NPCMappingInfo> infos)
        {
            foreach (Npc.NPCMappingInfo info in infos)
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
                List<Npc.NPCMappingInfo> infos = deserializer.Deserialize<List<Npc.NPCMappingInfo>>(sr);
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