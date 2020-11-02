using Exiled.API.Features;
using System.Collections.Generic;

namespace NPCS.AI.TargetFilters
{
    internal abstract class TargetFilter
    {
        public abstract string Name { get; }

        public abstract bool Check(Npc n, Player p);

        private static readonly Dictionary<string, TargetFilter> registry = new Dictionary<string, TargetFilter>();

        public static TargetFilter GetFromToken(string s)
        {
            return registry.ContainsKey(s) ? registry[s] : null;
        }

        public static void Register(TargetFilter f)
        {
            registry.Add(f.Name, f);
        }

        public static void Clear()
        {
            registry.Clear();
        }
    }
}