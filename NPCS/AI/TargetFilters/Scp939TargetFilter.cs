using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCS.AI.TargetFilters
{
    class Scp939TargetFilter : TargetFilter
    {
        public override bool Check(Npc n, Player p)
        {
            return p.GameObject.GetComponent<Scp939_VisionController>().CanSee(n.NPCPlayer.GameObject.GetComponent<Scp939PlayerScript>());
        }
    }
}
