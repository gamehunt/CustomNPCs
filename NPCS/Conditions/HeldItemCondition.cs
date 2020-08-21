using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Conditions
{
    internal class HeldItemCondition : NodeCondition
    {
        public override string Name => "HeldItemCondition";

        public override bool Check(Player player, Dictionary<string, string> args)
        {
            return player.Inventory.curItem == (ItemType)int.Parse(args["item"]);
        }
    }
}