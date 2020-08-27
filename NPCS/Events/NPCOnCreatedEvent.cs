using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Text;

namespace NPCS.Events
{
    class NPCOnCreatedEvent : NPCEvent
    {
        public NPCOnCreatedEvent(Npc npc, Player p) : base(npc, p)
        {
        }

        public override string Name => "NPCOnCreatedEvent";
    }
}
