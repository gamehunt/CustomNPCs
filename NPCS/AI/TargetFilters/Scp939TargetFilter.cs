using Exiled.API.Features;

namespace NPCS.AI.TargetFilters
{
    internal class Scp939TargetFilter : TargetFilter
    {
        public override string Name => "scp939";

        public override bool Check(Npc n, Player p)
        {
            return p.GameObject.GetComponent<Scp939_VisionController>().CanSee(n.NPCPlayer.GameObject.GetComponent<Scp939PlayerScript>());
        }
    }
}