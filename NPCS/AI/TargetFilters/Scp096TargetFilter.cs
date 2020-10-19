using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCS.AI.TargetFilters
{
    class Scp096TargetFilter : TargetFilter
    {
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
