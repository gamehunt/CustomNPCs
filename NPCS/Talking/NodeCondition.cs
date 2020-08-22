using Exiled.API.Features;
using System.Collections.Generic;

namespace NPCS.Talking
{
    internal abstract class NodeCondition
    {
        public abstract string Name { get; }

        public abstract bool Check(Player player, Dictionary<string, string> args);

        private static readonly Dictionary<string, NodeCondition> registry = new Dictionary<string, NodeCondition>();

        public static NodeCondition GetFromToken(string token)
        {
            try
            {
                return registry[token];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public static void Register(NodeCondition cond)
        {
            registry.Add(cond.Name, cond);
            Log.Debug($"Registered condition token: {cond.Name}", Plugin.Instance.Config.VerboseOutput);
        }
    }
}