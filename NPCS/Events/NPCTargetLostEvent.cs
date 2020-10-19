using Exiled.API.Features;

namespace NPCS.Events
{
    internal class NPCTargetLostEvent : NPCEvent
    {
        public NPCTargetLostEvent(Npc npc, Player p) : base(npc, p)
        {
        }

        public override string Name => "NPCTargetLostEvent";
    }
}