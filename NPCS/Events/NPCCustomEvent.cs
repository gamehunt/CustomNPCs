using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Text;

namespace NPCS.Events
{
    internal class NPCCustomEvent : NPCEvent
    {
        public NPCCustomEvent(Npc npc, Player p,string name) : base(npc, p)
        {
            mutable_name = name;
        }

        public override string Name => mutable_name;

        private readonly string mutable_name;
    }
}
