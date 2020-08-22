using Exiled.API.Features;
using NPCS.Talking;
using System;
using System.Collections.Generic;
using System.Text;

namespace NPCS.Actions
{
    class NavClearAction : NodeAction
    {
        public override string Name => "NavClearAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            npc.ClearNavTargets();
        }
    }
}
