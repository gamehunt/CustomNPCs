using Exiled.API.Features;

namespace NPCS.Events
{
    internal class NPCHurtEvent : NPCEvent
    {
        public NPCHurtEvent(Npc npc, Player p) : base(npc, p)
        {
        }

        public override string Name => "NPCHurtEvent";
    }
}