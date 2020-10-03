using Exiled.API.Features;
using NPCS.Talking;
using System;
using System.Collections.Generic;

namespace NPCS.Conditions
{
    internal class HasItemCondition : NodeCondition
    {
        public override string Name => "HasItemCondition";

        public override bool Check(Player player, Dictionary<string, string> args)
        {
            ItemType i_type = (ItemType)Enum.Parse(typeof(ItemType), args["item_type"]);
            foreach (Inventory.SyncItemInfo sii in player.Inventory.items)
            {
                if (sii.id == i_type)
                {
                    return true;
                }
            }
            return false;
        }
    }
}