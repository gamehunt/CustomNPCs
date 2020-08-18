using Exiled.API.Features;
using NPCS.Talking;
using System;
using System.Collections.Generic;

namespace NPCS.Conditions
{
    internal class HasEffectCondition : NodeCondition
    {
        public override string Name => "HasEffectCondition";

        public override bool Check(Player player, Dictionary<string, string> args)
        {
            foreach (KeyValuePair<Type, CustomPlayerEffects.PlayerEffect> keyValuePair in player.ReferenceHub.playerEffectsController.AllEffects)
            {
                if (string.Equals(keyValuePair.Key.ToString(), "customplayereffects." + args["type"], StringComparison.InvariantCultureIgnoreCase))
                {
                    CustomPlayerEffects.PlayerEffect effect = keyValuePair.Value;
                    return effect.Enabled;
                }
            }
            return false;
        }
    }
}