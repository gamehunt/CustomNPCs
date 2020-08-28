using Exiled.API.Features;

namespace NPCS.Events
{
    internal class NPCOnCreatedEvent : NPCEvent
    {
        public NPCOnCreatedEvent(Npc npc, Player p) : base(npc, p)
        {

        }

        public override string Name => "NPCOnCreatedEvent";
    }
}