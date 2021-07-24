using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCS.Commands.Nav
{
    class RemoveCommand : ICommand
    {
        public string Command => "remove";

        public string[] Aliases => new string[] { };

        public string Description => "Removes navigation node";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = null;
            return false;
        }
    }
}
