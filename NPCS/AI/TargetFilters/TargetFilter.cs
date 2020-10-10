using Exiled.API.Features;

namespace NPCS.AI.TargetFilters
{
    internal abstract class TargetFilter
    {
        public abstract bool Check(Npc n, Player p);
    }
}