using Exiled.API.Extensions;
using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

using UnityEngine;

namespace NPCS.Actions
{
    internal class DropItemAction : NodeAction
    {
        public override string Name => "DropItemAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            ((ItemType)int.Parse(args["item_type"])).Spawn(-4.656647E+11f, npc.PlayerInstance.Position, Quaternion.identity, 0, 0, 0);
        }
    }
}