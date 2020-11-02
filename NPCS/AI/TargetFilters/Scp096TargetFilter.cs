using Exiled.API.Features;

namespace NPCS.AI.TargetFilters
{
    internal class Scp096TargetFilter : TargetFilter
    {
        public override string Name => "scp096";

        public override bool Check(Npc n, Player p)
        {
            PlayableScps.Scp096 scp = n.NPCPlayer.ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096;
            if (scp != null)
            {
                return scp.Enraged && scp.HasTarget(p.ReferenceHub);
            }
            return false;
        }
    }
}