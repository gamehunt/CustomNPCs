using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCS.Commands.Nav
{
    class SaveCommand : ICommand
    {
        public string Command => "save";

        public string[] Aliases => new string[] { };

        public string Description => "Saves current graph";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = null;
            return false;
        }
    }
}