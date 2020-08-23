using Exiled.API.Features;
using MEC;
using NPCS.Talking;
using System;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class StopFollowAction : NodeAction
    {
        public override string Name => "StopFollowAction";

        private IEnumerator<float> StopAfterRoomCheck(Npc npc, string rname)
        {
            while (npc.NPCPlayer.CurrentRoom == null || !npc.NPCPlayer.CurrentRoom.Name.Equals(rname, StringComparison.OrdinalIgnoreCase))
            {
                yield return Timing.WaitForSeconds(0.1f);
            }
            npc.FollowTarget = null;
            Timing.KillCoroutines(npc.MovementCoroutines);
        }

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            if (args["room"] == "none")
            {
                npc.FollowTarget = null;
            }
            else
            {
                npc.AttachedCoroutines.Add(Timing.RunCoroutine(StopAfterRoomCheck(npc, args["room"])));
            }
        }
    }
}