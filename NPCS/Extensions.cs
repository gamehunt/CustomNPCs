using Exiled.API.Features;

namespace NPCS
{
    public static class Extensions
    {
        public static bool IsNPC(this Player p)
        {
            return Npc.Dictionary.ContainsKey(p.GameObject);
        }
    }
}