using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCS.Commands.Npc
{
    class SaveMappingsCommand : ICommand
    {
        public string Command => "save_mappings";

        public string[] Aliases => new string[] { "save_mappings" };

        public string Description => "Saves NPCs mappings to file";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = null;
            return false;
        }
    }
}
