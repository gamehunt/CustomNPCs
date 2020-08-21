using Exiled.API.Features;
using NPCS.Talking;
using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace NPCS.Actions
{
    class DropItemAction : NodeAction
    {
        public override string Name => "DropItemAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            PlayerManager.localPlayer.GetComponent<Inventory>().SetPickup((ItemType)int.Parse(args["item_type"]), -4.656647E+11f, npc.Position, Quaternion.identity, 0, 0, 0);
        }
    }
}
