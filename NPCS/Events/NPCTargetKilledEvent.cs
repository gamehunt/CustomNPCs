using Exiled.API.Features;

namespace NPCS.Events
{
    internal class NPCTargetKilledEvent : NPCEvent
    {
        public NPCTargetKilledEvent(Npc npc, Player p) : base(npc, p)
        {
        }

        public override string Name => "NPCTargetKilledEvent";
    }
}