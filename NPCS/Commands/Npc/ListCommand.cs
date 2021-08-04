using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCS.Commands.Npc
{
    class ListCommand : ICommand
    {
        public string Command => "list";

        public string[] Aliases => new string[] { "list" };

        public string Description => "Outputs list of npcs";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            foreach (NPCS.Npc npc in FakePlayers.API.FakePlayer.List.Where(n => n.IsNPC()))
            {
                sender.Respond($"{npc.PlayerInstance.Id} {npc.PlayerInstance.Nickname} {npc.GetIdentifier()}");
            }
            response = "List end";
            return true;
        }
    }
}
