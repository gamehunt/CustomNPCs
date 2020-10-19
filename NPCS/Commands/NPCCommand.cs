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
                if (arguments.Count == 0)
                {
                    response = "Available subcommands: [load, list, clean, remove, move, god, goto, queue, follow, room, sav_mappings, load_mappings]";
                    return false;
                }
                Npc obj_npc;
                NavigationNode node;
                string name;
                switch (arguments.At(0))
                {
                    case "load":
                        if (!s.IsAlive)
                        {
                            response = "You must be alive to use this!";
                            return false;
                        }
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
                            if (!npc.DontCleanup)
                            {
                                npc.Kill(false);
                            }
                        }
                        response = "NPCs cleaned";
                        break;

                    case "remove":
                        if (arguments.Count > 1)
                        {
                            try
                            {
                                obj_npc = Npc.List.ToList()[int.Parse(arguments.At(1))];
                                obj_npc.Kill(false);
                                response = "NPC removed!";
                            }
                            catch (IndexOutOfRangeException)
                            {
                                response = "Invalid NPC id!";
                                return false;
                            }
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
                            try
                            {
                                obj_npc = Npc.List.ToList()[int.Parse(arguments.At(1))];
                                obj_npc.NPCPlayer.Position += new UnityEngine.Vector3(float.Parse(arguments.At(2)), float.Parse(arguments.At(3)), float.Parse(arguments.At(4)));
                                response = "NPC moved!";
                            }
                            catch (IndexOutOfRangeException)
                            {
                                response = "Invalid NPC id!";
                                return false;
                            }
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
                        try
                        {
                            Npc.List.ToList()[int.Parse(arguments.At(1))].NPCPlayer.IsGodModeEnabled = bool.Parse(arguments.At(2));
                            response = "God-Mode switched";
                        }
                        catch (IndexOutOfRangeException)
                        {
                            response = "Invalid NPC id!";
                            return false;
                        }
                        break;

                    case "goto":
                        if (arguments.Count <= 2)
                        {
                            response = "You need to provide npc id and navnode name!";
                            return false;
                        }
                        try
                        {
                            name = arguments.At(2);
                            obj_npc = Npc.List.ToList()[int.Parse(arguments.At(1))];
                            node = NavigationNode.Get(name);
                            if (node == null)
                            {
                                response = "Node not found!";
                                return false;
                            }
                            obj_npc.GotoNode(node);
                            response = "Navigating npc to node!";
                        }
                        catch (IndexOutOfRangeException)
                        {
                            response = "Invalid NPC id!";
                            return false;
                        }
                        break;

                    case "queue":
                        if (arguments.Count <= 2)
                        {
                            response = "You need to provide npc id and navnode name!";
                            return false;
                        }
                        try
                        {
                            name = arguments.At(2);
                            obj_npc = Npc.List.ToList()[int.Parse(arguments.At(1))];
                            node = NavigationNode.Get(name);
                            if (node == null)
                            {
                                response = "Node not found!";
                                return false;
                            }
                            obj_npc.AddNavTarget(node);
                            response = "Navigating npc to node!";
                        }
                        catch (IndexOutOfRangeException)
                        {
                            response = "Invalid NPC id!";
                            return false;
                        }
                        break;

                    case "follow":
                        if (arguments.Count <= 2)
                        {
                            response = "You need to provide npc id and player id!";
                            return false;
                        }
                        try
                        {
                            int pid = int.Parse(arguments.At(2));
                            Player p = Player.Get(pid);
                            obj_npc = Npc.List.ToList()[int.Parse(arguments.At(1))];
                            obj_npc.Follow(p);
                            response = "Navigating npc to player!";
                        }
                        catch (IndexOutOfRangeException)
                        {
                            response = "Invalid NPC id!";
                            return false;
                        }
                        break;

                    case "room":
                        try
                        {
                            obj_npc = Npc.List.ToList()[int.Parse(arguments.At(1))];
                            Room r = Map.Rooms.Where(rm => rm.Name.Equals(arguments.At(2), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            if (r != null)
                            {
                                obj_npc.GotoRoom(r);
                            }
                            response = "Navigating npc to room!";
                        }
                        catch (IndexOutOfRangeException)
                        {
                            response = "Invalid NPC id!";
                            return false;
                        }
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