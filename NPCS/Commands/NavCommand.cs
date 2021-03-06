﻿using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using NPCS.Navigation;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

namespace NPCS.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class NavCommand : ICommand
    {
        public string Command => "nav";

        public string[] Aliases => new string[] { "nav" };

        public string Description => "Master command for NPCs navigation";

        private static int new_node_id = 0;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                Player s = Player.Get(player.PlayerId);
                if (!s.CheckPermission("npc.all"))
                {
                    response = "Access denied!";
                    return false;
                }
                if (!Round.IsStarted)
                {
                    response = "Round is not started!";
                    return false;
                }
                if (arguments.Count == 0)
                {
                    response = "Available subcommands: [create, list, remove, clean, rebuild, sav, show]";
                    return false;
                }
                NavigationNode created_node;
                Pickup pickup;
                switch (arguments.At(0))
                {
                    case "create":
                        string name = (++new_node_id).ToString();
                        if (!s.IsAlive)
                        {
                            response = "You must be alive to use this!";
                            return false;
                        }
                        created_node = NavigationNode.Create(s.Position, name, s.CurrentRoom.Name.RemoveBracketsOnEndOfName());
                        if (arguments.Count > 1)
                        {
                            string[] AvailableItemTypes = arguments.At(1).Split(',');
                            foreach (string type in AvailableItemTypes)
                            {
                                created_node.PossibleItemTypes.Add(type.Trim());
                            }
                        }
                        pickup = ItemType.SCP018.Spawn(1f, created_node.Position + new UnityEngine.Vector3(0, 0.5f, 0));
                        pickup.Locked = true;
                        foreach (NavigationNode d in NavigationNode.AllNodes.Values.Where(nd => nd != created_node && Vector3.Distance(nd.Position, created_node.Position) < Plugin.Instance.Config.NavNodeMapperMaxDistance))
                        {
                            created_node.LinkedNodes.Add(d);
                            d.LinkedNodes.Add(created_node);

                            Log.Info($"Linked {created_node.Name} and {d.Name}");
                        }
                        response = "Created node!";
                        break;

                    case "list":
                        int id = 0;
                        foreach (NavigationNode node in NavigationNode.AllNodes.Values)
                        {
                            s.RemoteAdminMessage($"{id} - {node.Name}");
                        }
                        response = "List end";
                        break;

                    case "remove":
                        if (arguments.Count <= 1)
                        {
                            response = "You need to provide node name!";
                            return false;
                        }
                        try
                        {
                            NavigationNode rnode = NavigationNode.AllNodes[arguments.At(1)];
                            NavigationNode.AllNodes.Remove(rnode.Name);
                            UnityEngine.Object.Destroy(rnode);
                            response = "Node removed";
                        }
                        catch (KeyNotFoundException)
                        {
                            response = "Node not found!";
                            return false;
                        }
                        break;

                    case "clean":
                        NavigationNode.Clear();
                        response = "Removed all nodes!";
                        break;

                    case "rebuild":
                        NavigationNode.Clear();
                        Methods.GenerateNavGraph();
                        response = "Rebuilt navigation graph";
                        break;

                    case "sav":
                        Dictionary<string, List<NavigationNode.NavNodeSerializationInfo>> manual_mappings = new Dictionary<string, List<NavigationNode.NavNodeSerializationInfo>>();
                        IEnumerable<NavigationNode> to_serialize = NavigationNode.AllNodes.Values.Where(n => n.SInfo != null);
                        foreach (NavigationNode node in to_serialize)
                        {
                            node.SInfo.ItemTypes = new List<string>(node.PossibleItemTypes);
                            if (!manual_mappings.ContainsKey(node.Room.RemoveBracketsOnEndOfName()))
                            {
                                List<NavigationNode.NavNodeSerializationInfo> nodes = new List<NavigationNode.NavNodeSerializationInfo>
                                {
                                    node.SInfo
                                };
                                manual_mappings.Add(node.Room.RemoveBracketsOnEndOfName(), nodes);
                            }
                            else
                            {
                                manual_mappings[node.Room.RemoveBracketsOnEndOfName()].Add(node.SInfo);
                            }
                        }
                        FileStream fs = File.Open(Config.NPCs_nav_mappings_path, FileMode.Truncate, FileAccess.Write);
                        StreamWriter sw = new StreamWriter(fs);
                        var serializer = new SerializerBuilder().Build();
                        var yaml = serializer.Serialize(manual_mappings);
                        sw.Write(yaml);
                        sw.Close();
                        response = "Saved manual navigation mappings!";
                        break;

                    case "show":
                        foreach (NavigationNode node in NavigationNode.AllNodes.Values)
                        {
                            pickup = ItemType.SCP018.Spawn(1f, node.Position + new UnityEngine.Vector3(0, 0.5f, 0));
                            pickup.Locked = true;
                        }
                        response = "Marked nodes!";
                        break;

                    case "items":
                        if (arguments.Count <= 2)
                        {
                            response = "You need to provide node name and at least one item group!";
                            return false;
                        }
                        try
                        {
                            NavigationNode rnode = NavigationNode.AllNodes[arguments.At(1)];
                            for (int i = 2; i < arguments.Count; i++)
                            {
                                if (rnode.PossibleItemTypes.Add(arguments.At(i)))
                                {
                                    s.RemoteAdminMessage($"Added type: {arguments.At(i)}");
                                }
                            }
                            response = "";
                        }
                        catch (KeyNotFoundException)
                        {
                            response = "Node not found!";
                            return false;
                        }
                        break;

                    case "r_items":
                        if (arguments.Count <= 2)
                        {
                            response = "You need to provide node name and at least one item group!";
                            return false;
                        }
                        try
                        {
                            NavigationNode rnode = NavigationNode.AllNodes[arguments.At(1)];
                            for (int i = 2; i < arguments.Count; i++)
                            {
                                if (rnode.PossibleItemTypes.Remove(arguments.At(i)))
                                {
                                    s.RemoteAdminMessage($"Removed type: {arguments.At(i)}");
                                }
                            }
                            response = "";
                        }
                        catch (KeyNotFoundException)
                        {
                            response = "Node not found!";
                            return false;
                        }
                        break;

                    case "info":
                        if (arguments.Count <= 1)
                        {
                            response = "You need to provide node name!";
                            return false;
                        }
                        try
                        {
                            NavigationNode rnode = NavigationNode.AllNodes[arguments.At(1)];
                            s.RemoteAdminMessage("Linked nodes:");
                            foreach (NavigationNode node in rnode.LinkedNodes)
                            {
                                s.RemoteAdminMessage(node.Name);
                            }
                            s.RemoteAdminMessage($"Attached door: {rnode.AttachedDoor}");
                            s.RemoteAdminMessage($"Attached elevator: {rnode.AttachedElevator}");
                            response = "";
                        }
                        catch (KeyNotFoundException)
                        {
                            response = "Node not found!";
                            return false;
                        }
                        break;

                    default:
                        response = "Unknown subcommand!";
                        return false;
                }
            }
            else
            {
                response = "Only players can use this!";
                return false;
            }
            return true;
        }
    }
}