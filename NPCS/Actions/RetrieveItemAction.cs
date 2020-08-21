using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class RetrieveItemAction : NodeAction
    {
        public override string Name => "RetrieveItemAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            if (player == null)
            {
                return;
            }
            ItemType i_type = (ItemType)int.Parse(args["item_type"]);
            foreach (Inventory.SyncItemInfo sii in player.Inventory.items)
            {
                if (sii.id == i_type)
                {
                    player.RemoveItem(sii);
                    break;
                }
            }
        }
    }
}