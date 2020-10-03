using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using NPCS.Navigation;
using RemoteAdmin;
using System;
using System.IO;
using System.Linq;

namespace NPCS.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class NPCCommand : ICommand
    {
        public string Command { get; } = "npc";

        public string[] Aliases { get; } = new string[] { "npc" };
        public string Description { get; } = "A master command of CustomNPCs plugin";

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
                if (!s.IsAlive)
                {
                    response = "You must be alive to use this!";
                    return false;
                }
                if (arguments.Count == 0)
                {
                    response = "Available subcommands: [create, list, remove, clean, load, save, god, goto]";
                    return false;
                }
                Npc obj_npc;
                NavigationNode node;
                string name;
                switch (arguments.At(0))
                {
                    case "load":
                        string file = arguments.Count < 2 ? "default_npc.yml" : arguments.At(1);
                        if (Methods.CreateNPC(s.Position, s.Rotations, file) == null)
                        {
                            response = "Failed to load NPC!";
                            return false;
                        }
                        response = "NPC loaded";
                        break;

                    case "list":
                        int id = 0;
                        foreach (Npc npc in Npc.List)
                        {
                            s.RemoteAdminMessage($"{id} | {npc.Name} | {Path.GetFileName(npc.RootNode.NodeFile)}", true, Plugin.Instance.Name);
                            id++;
                        }
                        response = "List ended";
                        break;

                    case "clean":
                        foreach (Npc npc in Npc.List)
                        {
                            npc.Kill(false);
                        }
                        response = "NPCs cleaned";
                        break;

                    case "remove":
                        if (arguments.Count > 1)
                        {
                            obj_npc = Npc.List.ToList()[int.Parse(arguments.At(1))];
                            obj_npc.Kill(false);
                            response = "NPC removed!";
                        }
                        else
                        {
                            response = "You need to provide NPC's id!";
                            return false;
                        }
                        break;

                    case "move":
                        if (arguments.Count > 4)
                        {
                            obj_npc = Npc.List.ToList()[int.Parse(arguments.At(1))];
                            obj_npc.NPCPlayer.Position += new UnityEngine.Vector3(float.Parse(arguments.At(2)), float.Parse(arguments.At(3)), float.Parse(arguments.At(4)));
                            response = "NPC moved!";
                        }
                        else
                        {
                            response = "You need to provide NPC's id and relatives to current position!";
                            return false;
                        }
                        break;

                    case "god":
                        if (arguments.Count <= 2)
                        {
                            response = "You need to provide npc id and godmode value!";
                            return false;
                        }
                        Npc.List.ToList()[int.Parse(arguments.At(1))].NPCPlayer.IsGodModeEnabled = bool.Parse(arguments.At(2));
                        response = "God-Mode switched";
                        break;

                    case "goto":
                        if (arguments.Count <= 2)
                        {
                            response = "You need to provide npc id and navnode name!";
                            return false;
                        }
                        name = arguments.At(2);
                        Npc npc_obj = Npc.List.ToList()[int.Parse(arguments.At(1))];
                        node = NavigationNode.Get(name);
                        if (node == null)
                        {
                            response = "Node not found!";
                            return false;
                        }
                        npc_obj.GotoNode(node);
                        response = "Navigating npc to node!";
                        break;

                    case "queue":
                        if (arguments.Count <= 2)
                        {
                            response = "You need to provide npc id and navnode name!";
                            return false;
                        }
                        name = arguments.At(2);
                        npc_obj = Npc.List.ToList()[int.Parse(arguments.At(1))];
                        node = NavigationNode.Get(name);
                        if (node == null)
                        {
                            response = "Node not found!";
                            return false;
                        }
                        npc_obj.AddNavTarget(node);
                        response = "Navigating npc to node!";
                        break;

                    case "follow":
                        if (arguments.Count <= 2)
                        {
                            response = "You need to provide npc id and player id!";
                            return false;
                        }
                        int pid = int.Parse(arguments.At(2));
                        Player p = Player.Get(pid);
                        npc_obj = Npc.List.ToList()[int.Parse(arguments.At(1))];
                        npc_obj.Follow(p);
                        response = "Navigating npc to player!";
                        break;

                    case "room":
                        npc_obj = Npc.List.ToList()[int.Parse(arguments.At(1))];
                        Room r = Map.Rooms.Where(rm => rm.Name.Equals(arguments.At(2), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (r != null)
                        {
                            npc_obj.GotoRoom(r);
                        }
                        response = "Navigating npc to room!";
                        break;

                    case "sav_mappings":

                        if (arguments.Count <= 1)
                        {
                            response = "You need to provide file name!";
                            return false;
                        }

                        Npc.SaveNPCMappings(arguments.At(1));
                        response = "Mappings saved!";
                        break;

                    case "load_mappings":

                        if (arguments.Count <= 1)
                        {
                            response = "You need to provide file name!";
                            return false;
                        }

                        Npc.LoadNPCMappings(arguments.At(1));
                        response = "Mappings loaded!";

                        break;

                    default:
                        response = "Unknown sub-command!";
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