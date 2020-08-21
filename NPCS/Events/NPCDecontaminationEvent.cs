using Exiled.API.Features;

namespace NPCS.Events
{
    internal class NPCDecontaminationEvent : NPCEvent
    {
        public override string Name => "NPCDecontaminationEvent";

        public NPCDecontaminationEvent(Npc npc, Player p) : base(npc, p)
        {
        }
    }
}