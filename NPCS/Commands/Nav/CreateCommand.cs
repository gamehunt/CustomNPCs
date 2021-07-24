using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCS.Commands.Nav
{
    class CreateCommand : ICommand
    {
        public string Command => "create";

        public string[] Aliases => new string[] { };

        public string Description => "Creates navigation node";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = null;
            return false;
        }
    }
}
