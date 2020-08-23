using Exiled.API.Features;

namespace NPCS.Events
{
    internal class NPCFollowTargetDiedEvent : NPCEvent
    {
        public override string Name => "NPCFollowTargetDiedEvent";

        public NPCFollowTargetDiedEvent(Npc npc, Player p) : base(npc, p)
        {
        }
    }
}