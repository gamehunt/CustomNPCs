using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class GiveItemAction : NodeAction
    {
        public override string Name => "GiveItemAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            if (player == null)
            {
                return;
            }
            player.AddItem((ItemType)int.Parse(args["item_type"]));
        }
    }
}