using Exiled.API.Features;
using System;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class NukeStopAction : Talking.NodeAction
    {
        public override string Name => "NukeStopAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            Warhead.Stop();
        }
    }
}