using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using NPCS.Commands.Nav;
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
    internal class NavCommand : ParentCommand
    {
        public NavCommand() => LoadGeneratedCommands();
        public override string Command => "nav";

        public override string[] Aliases => new string[] { "nav" };

        public override string Description => "Master command for NPCs navigation";

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                Player s = Player.Get(player.PlayerId);
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
            }
            else
            {
                response = "Only players can use this!";
                return false;
            }
            response = null;
            return true;
        }

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new CreateCommand());
            RegisterCommand(new RemoveCommand());
            RegisterCommand(new LinkCommand());
            RegisterCommand(new LoadCommand());
            RegisterCommand(new SaveCommand());
        }
    }
}