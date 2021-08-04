using CommandSystem;
using System;
using System.Linq;

namespace NPCS.Commands.Npc
{
    class RemoveCommand : ICommand
    {
        public string Command => "clean";

        public string[] Aliases => new string[] { "clean" };

        public string Description => "Cleans all NPCs";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            foreach(NPCS.Npc npc in FakePlayers.API.FakePlayer.List.Where(n => n.IsNPC()))
            {
                npc.Kill();
            }
            response = "All NPCs have been destroyed";
            return true;
        }
    }
}
