using Exiled.API.Features;
using NPCS.Talking;
using System;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class DisableEffectAction : NodeAction
    {
        public override string Name => "DisableEffectAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            if (player == null)
            {
                return;
            }
            foreach (KeyValuePair<Type, CustomPlayerEffects.PlayerEffect> keyValuePair in player.ReferenceHub.playerEffectsController.AllEffects)
            {
                if (string.Equals(keyValuePair.Key.ToString(), "customplayereffects." + args["type"], StringComparison.InvariantCultureIgnoreCase))
                {
                    CustomPlayerEffects.PlayerEffect effect = keyValuePair.Value;
                    effect.ServerDisable();
                }
            }
        }
    }
}