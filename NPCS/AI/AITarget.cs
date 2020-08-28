using Exiled.API.Features;
using System;
using System.Collections.Generic;

namespace NPCS.AI
{
    internal abstract class AITarget
    {
        public abstract string Name { get; }

        public Dictionary<string, string> Arguments { get; set; } = new Dictionary<string, string>();

        public bool IsFinished { get; protected set; } = false;

        public abstract float Process(Npc npc);

        public abstract bool Check(Npc npc);

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