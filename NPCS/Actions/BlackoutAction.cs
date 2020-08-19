using Exiled.API.Features;
using System;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class BlackoutAction : Talking.NodeAction
    {
        public override string Name => "BlackoutAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            Map.TurnOffAllLights(float.Parse(args["duration"]), bool.Parse(args["hcz_only"]));
        }
    }
}