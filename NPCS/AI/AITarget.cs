using Exiled.API.Features;
using System;
using System.Collections.Generic;

namespace NPCS.AI
{
    public abstract class AITarget
    {
        public abstract string Name { get; }

        public abstract string[] RequiredArguments { get; }

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
                try
                {
                    string missing = CheckArguments();
                    Verified = missing.Length == 0;
                    if (Verified)
                    {
                        Construct();
                    }
                    else
                    {
                        Log.Warn($"AITarget {Name} missing required '{missing}' argument and will be skipped!");
                    }
                }
                catch (Exception e)
                {
                    Log.Warn($"Error while constructing AI target: {e}");
                    Verified = false;
                }
            }
        }

        public bool IsFinished { get; protected set; } = false;

        public bool Verified { get; protected set; } = false;

        public abstract float Process(Npc npc);

        public abstract bool Check(Npc npc);

        public abstract void Construct();

        private string CheckArguments()
        {
            foreach (string arg in RequiredArguments)
            {
                if (!Arguments.ContainsKey(arg))
                {
                    return arg;
                }
            }
            return "";
        }

        private static readonly Dictionary<string, AITarget> registry = new Dictionary<string, AITarget>();

        public static AITarget GetFromToken(string token)
        {
            try
            {
                return registry[token].CreateInstance();
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