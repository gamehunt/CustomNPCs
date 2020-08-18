using Exiled.API.Features;
using System.Collections.Generic;

namespace NPCS.Talking
{
    internal abstract class NodeAction
    {
        public abstract string Name { get; }

        public abstract void Process(NPCS.Npc npc, Player player, Dictionary<string, string> args);

        private static List<NodeAction> registry = new List<NodeAction>();

        public static NodeAction GetFromToken(string token)
        {
            return registry.Find(c => c.Name.Equals(token));
        }

        public static void Register(NodeAction cond)
        {
            registry.Add(cond);
            Log.Debug($"Registered action token: {cond.Name}",Plugin.Instance.Config.VerboseOutput);
        }
    }
}