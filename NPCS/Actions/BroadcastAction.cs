using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class BroadcastAction : NodeAction
    {
        public override string Name => "BroadcastAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            if (bool.Parse(args["global"]))
            {
                Map.Broadcast(5, args["text"]);
            }
            else if(player != null)
            {
                player.Broadcast(5, args["text"]);
            }
        }
    }
}