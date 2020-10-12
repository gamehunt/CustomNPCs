using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCS.Events
{
    class NPCTargetLostEvent : NPCEvent
    {
        public NPCTargetLostEvent(Npc npc, Player p) : base(npc, p)
        {
        }

        public override string Name => "NPCTargetLostEvent";
    }
}
