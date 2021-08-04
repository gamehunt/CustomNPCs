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
            if(arguments.Count < 1)
            {
                response = "NPC id required!";
                return false;
            }
            NPCS.Npc npc = (NPCS.Npc)FakePlayers.API.FakePlayer.List.Where(n => n.IsNPC() && n.PlayerInstance.Id == int.Parse(arguments.Array[0]));
            if (npc != null)
            {
                npc.Kill();
                response = "Removed NPC";
                return true;
            }
            else
            {
                response = "NPC not found!";
                return false;
            }
        }
    }
}
