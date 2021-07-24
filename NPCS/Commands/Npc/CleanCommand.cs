using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCS.Commands.Npc
{
    class RemoveCommand : ICommand
    {
        public string Command => "clean";

        public string[] Aliases => new string[] { "clean" };

        public string Description => "Cleans all NPCs";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = null;
            return false;
        }
    }
}
