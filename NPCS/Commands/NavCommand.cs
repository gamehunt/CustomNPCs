using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using NPCS.Navigation;
using RemoteAdmin;
using System;
using System.Collections.Generic;

namespace NPCS.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class NavCommand : ICommand
    {
        public string Command => "nav";

        public string[] Aliases => new string[] { "nav" };

        public string Description => "Master command for NPCs navigation";

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
                    response = "Available subcommands: [create, list, remove, clean]";
                    return false;
                }
                switch (arguments.At(0))
                {
                    case "create":
                        if (arguments.Count <= 1)
                        {
                            response = "You need to provide node name!";
                            return false;
                        }
                        if (NavigationNode.Get(arguments.At(1)) != null)
                        {
                            response = "Node with this name already exists!";
                            return false;
                        }
                        NavigationNode.Create(s.Position, arguments.At(1));
                        response = "Created node!";
                        break;

                    case "list":
                        int id = 0;
                        foreach (NavigationNode node in NavigationNode.AllNodes.Values)
                        {
                            s.RemoteAdminMessage($"{id} - {node.Name} - {node.Priority}");
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
                        } catch (KeyNotFoundException e)
                        {
                            response = "Node not found!";
                            return false;
                        }
                        break;

                    case "clean":
                        NavigationNode.Clear();
                        response = "Removed all nodes!";
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