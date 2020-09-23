using Exiled.API.Features;
using System.Collections.Generic;

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