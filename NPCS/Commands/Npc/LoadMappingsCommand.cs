using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCS.Commands.Npc
{
    class LoadMappingsCommand : ICommand
    {
        public string Command => "load_mappings";

        public string[] Aliases => new string[] { "load_mappings" };

        public string Description => "Loads npcs from file";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = null;
            return false;
        }
    }
}
