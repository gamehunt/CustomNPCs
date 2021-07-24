using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCS.Commands.Npc
{
    class LoadCommand : ICommand
    {
        public string Command => "load";

        public string[] Aliases => new string[] { "load" };

        public string Description => "Loads NPC from file";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "123";
            return false;
        }
    }
}
