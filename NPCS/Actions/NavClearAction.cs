﻿using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class NavClearAction : NodeAction
    {
        public override string Name => "NavClearAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            npc.ClearNavTargets();
        }
    }
}