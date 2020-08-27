using Exiled.API.Features;

namespace NPCS.Events
{
    internal class NPCTeamRespawnEvent : NPCEvent
    {
        public Respawning.SpawnableTeamType Team { get; }

        public NPCTeamRespawnEvent(Npc npc, Player p, Respawning.SpawnableTeamType team) : base(npc, p)
        {
            Team = team;
        }

        public override string Name => "NPCTeamRespawnEvent";

        public override void OnFired(Npc npc)
        {
            if (Team == Respawning.SpawnableTeamType.NineTailedFox)
            {
                npc.FireEvent(new NPCCustomEvent(npc, null, "NPCNTFRespawnEvent"));
            }
            else
            {
                npc.FireEvent(new NPCCustomEvent(npc, null, "NPCCIRespawnEvent"));
            }
        }
    }
}