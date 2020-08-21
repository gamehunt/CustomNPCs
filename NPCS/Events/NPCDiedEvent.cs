using Exiled.API.Features;

namespace NPCS.Events
{
    internal class NPCDiedEvent : NPCEvent
    {
        public override string Name => "NPCDied";

        public NPCDiedEvent(Npc npc, Player p) : base(npc, p)
        {
        }
    }
}