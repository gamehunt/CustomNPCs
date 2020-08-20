using Exiled.API.Features;
using System.Collections.Generic;

namespace NPCS.Talking
{
    internal abstract class NodeCondition
    {
        public abstract string Name { get; }

        public abstract bool Check(Player player, Dictionary<string, string> args);

        private static readonly List<NodeCondition> registry = new List<NodeCondition>();

        public static NodeCondition GetFromToken(string token)
        {
            return registry.Find(c => c.Name.Equals(token));
        }

        public static void Register(NodeCondition cond)
        {
            registry.Add(cond);
            Log.Debug($"Registered condition token: {cond.Name}", Plugin.Instance.Config.VerboseOutput);
        }
    }
}