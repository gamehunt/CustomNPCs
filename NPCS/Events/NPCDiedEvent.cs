using Exiled.API.Features;

namespace NPCS.Events
{
    internal class NPCDiedEvent : NPCEvent
    {
        public override string Name => "NPCDiedEvent";

        public NPCDiedEvent(Npc npc, Player p) : base(npc, p)
        {
        }
    }
}