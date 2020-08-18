using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class GiveHealthAction : NodeAction
    {
        public override string Name => "GiveHealthAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            player.Health += float.Parse(args["amount"]);
        }
    }
}