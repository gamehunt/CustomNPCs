using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class ConsumeHealthAction : NodeAction
    {
        public override string Name => "ConsumeHealthAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            player.Health -= float.Parse(args["amount"]);
        }
    }
}