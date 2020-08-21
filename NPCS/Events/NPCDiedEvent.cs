using System;
using System.Collections.Generic;
using System.Text;
using Exiled.API.Features;
using NPCS.Actions;
using NPCS.Talking;

namespace NPCS.Events
{
    internal class NPCDiedEvent : NPCEvent
    {
        public override string Name => "NPCDied";

        public NPCDiedEvent(Npc npc,Player p): base(npc, p)
        {
        }
    }
}
