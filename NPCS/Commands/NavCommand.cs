using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using NPCS.Navigation;
using RemoteAdmin;
using System;

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
                switch (arguments.At(0))
                {
                    case "create":
                        if (arguments.Count <= 1)
                        {
                            response = "You need to provide node name!";
                            return false;
                        }
                        NavigationNode.Create(s.Position, arguments.At(1));
                        response = "Created node!";
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