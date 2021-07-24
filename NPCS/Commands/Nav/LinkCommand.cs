using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCS.Commands.Nav
{
    class LinkCommand : ICommand
    {
        public string Command => "links";

        public string[] Aliases => new string[] { };

        public string Description => "Links navigation nodes and objects";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = null;
            return false;
        }
    }
}
