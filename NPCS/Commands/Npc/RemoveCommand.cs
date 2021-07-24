using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCS.Commands.Npc
{
    class CleanCommand : ICommand
    {
        public string Command => "remove";

        public string[] Aliases => new string[] { "remove" };

        public string Description => "Removes NPC by id";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = null;
            return false;
        }
    }
}
