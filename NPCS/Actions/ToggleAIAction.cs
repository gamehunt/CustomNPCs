using Exiled.API.Features;
using NPCS.Talking;
using System;
using System.Collections.Generic;
using System.Text;

namespace NPCS.Actions
{
    class ToggleAIAction : NodeAction
    {
        public override string Name => "ToggleAIAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            npc.AIEnabled = bool.Parse(args["value"]);
        }
    }
}
