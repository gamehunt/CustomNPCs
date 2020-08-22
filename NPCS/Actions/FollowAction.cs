using Exiled.API.Features;
using NPCS.Talking;
using System;
using System.Collections.Generic;
using System.Text;

namespace NPCS.Actions
{
    class FollowAction : NodeAction
    {
        public override string Name => "FollowAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            npc.Follow(player);
        }
    }
}
