﻿using Exiled.API.Features;
using MEC;
using Mirror;
using NPCS.AI;
using NPCS.Events;
using NPCS.Talking;
using NPCS.Navigation;
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
    internal class Methods
    {
        public static Npc CreateNPC(Vector3 pos, Vector2 rot, RoleType type = RoleType.ClassD, ItemType itemHeld = ItemType.None, string name = "(EMPTY)", string root_node = "default_node.yml")
        {
            GameObject obj =
                UnityEngine.Object.Instantiate(
                    NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
            CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();

            obj.transform.localScale = Vector3.one;
            obj.transform.position = pos;

            obj.GetComponent<QueryProcessor>().NetworkPlayerId = QueryProcessor._idIterator++;
            obj.GetComponent<QueryProcessor>()._ipAddress = "127.0.0.WAN";

            if (Plugin.Instance.Config.DisplayNpcInPlayerList)
            {
                ccm._privUserId = $"{name}-{obj.GetComponent<QueryProcessor>().PlayerId }@NPC";
            }

            ccm.CurClass = type;
            obj.GetComponent<PlayerStats>().SetHPAmount(ccm.Classes.SafeGet(type).maxHP);

            obj.GetComponent<NicknameSync>().Network_myNickSync = name;

            obj.GetComponent<ServerRoles>().MyText = "NPC";
            obj.GetComponent<ServerRoles>().MyColor = "red";

            NetworkServer.Spawn(obj);
            PlayerManager.AddPlayer(obj); //I'm not sure if I need this

            Player ply_obj = new Player(obj);
            Player.Dictionary.Add(obj, ply_obj);

            Player.IdsCache.Add(ply_obj.Id, ply_obj);

            if (Plugin.Instance.Config.DisplayNpcInPlayerList)
            {
                Player.UserIdsCache.Add(ccm._privUserId, ply_obj);
            }

            Npc npcc = obj.AddComponent<Npc>();

            npcc.ItemHeld = itemHeld;
            npcc.RootNode = TalkNode.FromFile(Path.Combine(Config.NPCs_nodes_path, root_node));

            npcc.AttachedCoroutines.Add(Timing.CallDelayed(0.3f, () =>
            {
                npcc.NPCPlayer.Position = pos;
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
        //role: 6
        //item_held: 24
        //root_node: default_node.yml
        //god_mode: false
        //is_exclusive: true
        //events: []
        //ai_enabled: false
        //ai: []
        public static Npc CreateNPC(Vector3 pos, Vector2 rot, string path)
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
                    n.NPCPlayer.IsGodModeEnabled = true;
                }
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

        public static void GenerateNavGraph()
        {
            Log.Info("[NAV] Generating navigation graph...");
            foreach (Room r in Map.Rooms)
            {
                NavigationNode node = NavigationNode.Create(r.Position, $"AUTO_Room_{r.Name}".Replace(' ', '_'));
                foreach (Door d in r.GetDoors())
                {
                    //Log.Info("HERE");
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
                    //Log.Debug($"[NAV] Linked door {new_node.Name} node to room {r.Name}", Plugin.Instance.Config.VerboseOutput);
                }
            }
        }


    }
}