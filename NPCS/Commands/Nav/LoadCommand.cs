using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCS.Commands.Nav
{
    class LoadCommand : ICommand
    {
        public string Command => "load";

        public string[] Aliases => new string[] { };

        public string Description => "Loads navigation nodes";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = null;
            return false;
        }
    }
}