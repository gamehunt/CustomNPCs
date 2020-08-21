using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class EnableEffectAction : NodeAction
    {
        public override string Name => "EnableEffectAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            if (player == null)
            {
                return;
            }
            if (!args.ContainsKey("duration"))
            {
                player.ReferenceHub.playerEffectsController.EnableByString(args["effect"]);
            }
            else
            {
                player.ReferenceHub.playerEffectsController.EnableByString(args["effect"], float.Parse(args["duration"]));
            }
        }
    }
}