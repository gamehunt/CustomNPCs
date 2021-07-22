using Exiled.API.Features;
using UnityEngine;

namespace NPCS.AI.TargetFilters
{
    internal class CommonTargetFilter : TargetFilter
    {
        public override string Name => "common";

        public override bool Check(Npc n, Player p)
        {
            return !n.PlayerInstance.TargetGhostsHashSet.Contains(p.Id) && !n.PlayerInstance.TargetGhosts.Contains(p.Id) && !p.IsInvisible && !Physics.Linecast(n.PlayerInstance.Position, p.Position, n.PlayerInstance.ReferenceHub.playerMovementSync.CollidableSurfaces);
        }
    }
}