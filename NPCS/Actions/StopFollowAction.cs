using Exiled.API.Features;
using MEC;
using NPCS.Talking;
using System;
using System.Collections.Generic;
using System.Text;

namespace NPCS.Actions
{
    class StopFollowAction : NodeAction
    {
        public override string Name => "StopFollowAction";

        private IEnumerator<float> StopAfterRoomCheck(Npc npc, string rname)
        {
            Player p = Player.Get(npc.GameObject);
            while(p.CurrentRoom == null || !p.CurrentRoom.Name.Equals(rname,StringComparison.OrdinalIgnoreCase))
            {
                yield return Timing.WaitForSeconds(0.1f);
            }
            npc.FollowTarget = null;
            Timing.KillCoroutines(npc.NPCComponent.movement_coroutines);
        }

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            if (args["room"] == "none")
            {
                npc.FollowTarget = null;
            }
            else
            {
                npc.NPCComponent.attached_coroutines.Add(Timing.RunCoroutine(StopAfterRoomCheck(npc, args["room"])));
            }
        }
    }
}
