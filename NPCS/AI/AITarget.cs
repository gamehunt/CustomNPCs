using Exiled.API.Features;
using System.Collections.Generic;
using YamlDotNet.Core.Tokens;

namespace NPCS.AI
{
    public abstract class AITarget
    {
        public abstract string Name { get; }

        private Dictionary<string, string> __args = new Dictionary<string, string>();

        public Dictionary<string, string> Arguments
        {
            get
            {
                return __args;
            }
            set
            {
                __args = value;
                Contruct();
            }
        }

        public bool IsFinished { get; protected set; } = false;

        public abstract float Process(Npc npc);

        public abstract bool Check(Npc npc);

        public abstract void Contruct();

        private static readonly Dictionary<string, AITarget> registry = new Dictionary<string, AITarget>();

        public static AITarget GetFromToken(string token)
        {
            try
            {
                return (AITarget)registry[token].CreateInstance();
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public static void Register(AITarget cond)
        {
            registry.Add(cond.Name, cond);
            Log.Debug($"Registered AITarget token: {cond.Name}", Plugin.Instance.Config.VerboseOutput);
        }

        public static void Clear()
        {
            Log.Debug($"Clearing AITarget registries...", Plugin.Instance.Config.VerboseOutput);
            registry.Clear();
        }

        protected abstract AITarget CreateInstance();
    }
}