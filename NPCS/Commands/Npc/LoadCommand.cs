using CommandSystem;
using System;
using FakePlayers.API;
using RemoteAdmin;
using Exiled.API.Features;

namespace NPCS.Commands.Npc
{
    class LoadCommand : ICommand
    {
        public string Command => "load";

        public string[] Aliases => new string[] { "load" };

        public string Description => "Loads NPC from file";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            PlayerCommandSender ply = (PlayerCommandSender)sender;
            Player ply_obj = Player.Get(ply.PlayerId);
            NPCS.Npc npc = Methods.LoadNPC(ply_obj.Position, ply_obj.Rotations, arguments.Count > 0 ? arguments.Array[0] : "default_npc.yml");
            if (npc != null)
            {
                response = $"NPC loaded! Debug identifier: {npc.GetIdentifier()}";
                return true;
            }
            else
            {
                response = "Failed to load NPC! Check console for details";
                return false;
            }
        }
    }
}
