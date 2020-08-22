using Exiled.API.Features;
using NPCS.Talking;
using System;
using System.Collections.Generic;
using System.Text;

using NPCS.Events;

namespace NPCS.Actions
{
    class FireEventAction : NodeAction
    {
        public override string Name => "FireEventAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            NPCCustomEvent ev = new NPCCustomEvent(npc, player, args["event_name"]);
            npc.FireEvent(ev);
        }
    }
}
