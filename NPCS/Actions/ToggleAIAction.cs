using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class ToggleAIAction : NodeAction
    {
        public override string Name => "ToggleAIAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            npc.AIEnabled = bool.Parse(args["value"]);
        }
    }
}