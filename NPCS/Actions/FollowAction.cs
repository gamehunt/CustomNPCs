using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class FollowAction : NodeAction
    {
        public override string Name => "FollowAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            npc.DisableFollowAutoTeleport = false;
            npc.Follow(player);
        }
    }
}