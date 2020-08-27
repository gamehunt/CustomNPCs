using Exiled.API.Features;

namespace NPCS.Events
{
    internal class NPCWarheadStartedEvent : NPCEvent
    {
        public NPCWarheadStartedEvent(Npc npc, Player p) : base(npc, p)
        {
        }

        public override string Name => "NPCWarheadStartedEvent";
    }
}