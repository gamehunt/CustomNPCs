using Exiled.API.Features;
using UnityEngine;

namespace NPCS.AI.TargetFilters
{
    internal class CommonTargetFilter : TargetFilter
    {
        public override string Name => "common";

        public override bool Check(Npc n, Player p)
        {
            return !n.NPCPlayer.TargetGhostsHashSet.Contains(p.Id) && !n.NPCPlayer.TargetGhosts.Contains(p.Id) && !p.IsInvisible && !Physics.Linecast(n.NPCPlayer.Position, p.Position, n.NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces);
        }
    }
}