using Exiled.API.Extensions;
using Exiled.API.Features;
using FakePlayers.API;
using Interactables.Interobjects.DoorUtils;
using MEC;
using NPCS.Navigation;
using NPCS.Talking;
using NPCS.Utils;
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
        public static Npc LoadNPC(Vector3 pos, Vector2 rotation, string file)
        {
            var input = new StringReader(File.ReadAllText(Path.Combine(Config.RootDirectory, file)));

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

            if (raw_npc.ProcessEvents)
            {
                Log.Warn("Your NPC have process_events set to true, so EXILED and some plugins can produce NRE's/incorrect behaviour");
                Log.Warn("Make sure you are really need this flag before using!");
            }

            Npc npc = FakePlayer.Create<Npc>(pos, new Vector3(raw_npc.Scale[0], raw_npc.Scale[1], raw_npc.Scale[2]), raw_npc.Role, raw_npc.ProcessEvents);

            Timing.CallDelayed(0.5f, () =>
            {
                try
                {
                    npc.IsExclusive = raw_npc.IsExclusive;
                    npc.AIEnabled = raw_npc.AiEnabled;

                    string full = Path.Combine(Config.ScriptsDirectory, raw_npc.AiScript);
                    Log.Debug($"Enabling script: {full}", Plugin.Instance.Config.VerboseOutput);
                    npc.AIScript = full;

                    foreach (ItemType type in raw_npc.Inventory)
                    {
                        npc.PlayerInstance.AddItem(type);
                    }

                    npc.PlayerInstance.Rotations = rotation;
                    npc.PlayerInstance.Health = raw_npc.Health <= 0 ? npc.PlayerInstance.ReferenceHub.characterClassManager.Classes.SafeGet(raw_npc.Role).maxHP : raw_npc.Health;
                    npc.PlayerInstance.ReferenceHub.nicknameSync.Network_myNickSync = raw_npc.Name;
                    npc.PlayerInstance.RankName = "NPC";
                    npc.PlayerInstance.Inventory.Network_curItemSynced = raw_npc.ItemHeld;
                    npc.PlayerInstance.IsGodModeEnabled = raw_npc.GodMode;
                    npc.AffectEndConditions = raw_npc.AffectSummary;
                    npc.RootNode = TalkNode.FromFile(Path.Combine(Config.DialogNodesDirectory, raw_npc.RootNode));

                    npc.StartAI();
                }
                catch(Exception e)
                {
                    Log.Error($"Exception in NPC initializer: {e}");
                }
            });
            
            return npc;
        }

        public static Npc LoadNPC(Npc.NPCMappingInfo info)
        {
            Room rm = Map.Rooms.Where(r => r.Name.Equals(info.Room, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (rm != null)
            {
                Vector3 position = rm.Position + Quaternion.Euler(0, rm.Transform.localRotation.eulerAngles.y - info.RoomRotation, 0) * info.Relative.ToVector3();
                Vector2 rotation = info.Rotation.ToVector2() + new Vector2(0, rm.Transform.localRotation.eulerAngles.y - info.RoomRotation);
                Npc npc = LoadNPC(position, rotation, info.File);
                return npc;
            }
            return null;
        }

        //shitcode
        public static void GenerateNavGraph()
        {
            try
            {
                Log.Info("[NAV] Generating navigation graph...");

                StreamReader sr = File.OpenText(Config.NavMappingsDirectory);
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
            path = Path.Combine(Config.MappingsDirectory, path);
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
                LoadNPC(info);
                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        public static void LoadNPCMappings(string path)
        {
            path = Path.Combine(Config.MappingsDirectory, path);
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