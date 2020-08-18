using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Conditions
{
    internal class HasntItemCondition : NodeCondition
    {
        public override string Name => "HasntItemCondition";

        public override bool Check(Player player, Dictionary<string, string> args)
        {
            ItemType i_type = (ItemType)int.Parse(args["item_type"]);
            foreach (Inventory.SyncItemInfo sii in player.Inventory.items)
            {
                if (sii.id == i_type)
                {
                    return false;
                }
            }
            return true;
        }
    }
}