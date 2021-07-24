using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using NPCS.Commands.Npc;
using NPCS.Navigation;
using RemoteAdmin;
using System;
using System.IO;
using System.Linq;

namespace NPCS.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class NPCCommand : ParentCommand
    {
        public NPCCommand() => LoadGeneratedCommands();
        public override string Command { get; } = "npc";

        public override string[] Aliases { get; } = new string[] { "npc" };
        public override string Description { get; } = "A master command of CustomNPCs plugin";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new LoadCommand());
            RegisterCommand(new ListCommand());
            RegisterCommand(new CleanCommand());
            RegisterCommand(new RemoveCommand());
            RegisterCommand(new SaveMappingsCommand());
            RegisterCommand(new LoadMappingsCommand());
        }

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
                    response = "Available subcommands: [load, list, clean, remove, save_mappings, load_mappings]";
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
    }
}