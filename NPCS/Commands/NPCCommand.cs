using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using NPCS.Navigation;
using RemoteAdmin;
using System;
using System.Collections.Generic;
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
                    case "create":

                        if (arguments.Count == 1)
                        {
                            Methods.CreateNPC(s.Position, s.Rotations, "default_npc.yml");
                        }
                        else if (arguments.Count == 2)
                        {
                            Methods.CreateNPC(s.Position, s.Rotations, RoleType.Scientist, ItemType.None, arguments.At(1));
                        }
                        else if (arguments.Count == 3)
                        {
                            Methods.CreateNPC(s.Position, s.Rotations, (RoleType)int.Parse(arguments.At(2)), ItemType.None, arguments.At(1));
                        }
                        else if (arguments.Count == 4)
                        {
                            Methods.CreateNPC(s.Position, s.Rotations, (RoleType)int.Parse(arguments.At(2)), (ItemType)int.Parse(arguments.At(3)), arguments.At(1));
                        }
                        else if (arguments.Count == 5)
                        {
                            Methods.CreateNPC(s.Position, s.Rotations, (RoleType)int.Parse(arguments.At(2)), (ItemType)int.Parse(arguments.At(3)), arguments.At(1), arguments.At(4));
                        }
                        response = "NPC created";
                        break;

                    case "load":
                        if (arguments.Count < 2)
                        {
                            response = "You need to provide path to file!";
                            return false;
                        }
                        if (Methods.CreateNPC(s.Position, s.Rotations, arguments.At(1)) == null)
                        {
                            response = "Failed to load NPC!";
                            return false;
                        }
                        response = "NPC loaded";
                        break;

                    case "save":
                        if (arguments.Count < 3)
                        {
                            response = "You need to provide npc id and path to file!";
                            return false;
                        }
                        obj_npc = Npc.List[int.Parse(arguments.At(1))];
                        try
                        {
                            obj_npc.Serialize(arguments.At(2));
                        }
                        catch (Exception)
                        {
                            response = "Failed to save NPC!";
                            return false;
                        }
                        response = "NPC saved";
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
                        List<Npc> list = Npc.List;
                        foreach (Npc npc in list)
                        {
                            npc.Kill(false);
                        }
                        response = "NPCs cleaned";
                        break;

                    case "remove":
                        if (arguments.Count > 1)
                        {
                            obj_npc = Npc.List[int.Parse(arguments.At(1))];
                            obj_npc.Kill(false);
                            response = "NPC removed!";
                        }
                        else
                        {
                            response = "You need to provide NPC's id!";
                            return false;
                        }
                        break;

                    case "god":
                        if (arguments.Count <= 2)
                        {
                            response = "You need to provide npc id and godmode value!";
                            return false;
                        }
                        Npc.List[int.Parse(arguments.At(1))].NPCPlayer.IsGodModeEnabled = bool.Parse(arguments.At(2));
                        response = "God-Mode switched";
                        break;

                    case "goto":
                        if (arguments.Count <= 2)
                        {
                            response = "You need to provide npc id and navnode name!";
                            return false;
                        }
                        name = arguments.At(2);
                        Npc npc_obj = Npc.List[int.Parse(arguments.At(1))];
                        node = NavigationNode.Get(name);
                        if (node == null)
                        {
                            response = "Node not found!";
                            return false;
                        }
                        npc_obj.CurrentNavTarget = node;
                        npc_obj.GoTo(node.Position);
                        response = "Navigating npc to node!";
                        break;

                    case "queue":
                        if (arguments.Count <= 2)
                        {
                            response = "You need to provide npc id and navnode name!";
                            return false;
                        }
                        name = arguments.At(2);
                        npc_obj = Npc.List[int.Parse(arguments.At(1))];
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
                        npc_obj = Npc.List[int.Parse(arguments.At(1))];
                        npc_obj.Follow(p);
                        response = "Navigating npc to player!";
                        break;

                    case "room":
                        npc_obj = Npc.List[int.Parse(arguments.At(1))];
                        Room r = Map.Rooms.Where(rm => rm.Name.Equals(arguments.At(2), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (r != null)
                        {
                            npc_obj.GotoRoom(r);
                        }
                        response = "Navigating npc to room!";
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