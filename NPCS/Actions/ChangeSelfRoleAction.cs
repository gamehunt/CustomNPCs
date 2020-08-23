using Exiled.API.Features;
using MEC;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class ChangeSelfRoleAction : Talking.NodeAction
    {
        public override string Name => "ChangeSelfRoleAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            npc.NPCPlayer.Role = (RoleType)int.Parse(args["role"]);
            if (!bool.Parse(args["preserve_position"]))
            {
                npc.AttachedCoroutines.Add(Timing.CallDelayed(0.1f, () =>
                {
                    npc.NPCPlayer.Position = (Map.GetRandomSpawnPoint(npc.NPCPlayer.Role));
                }));
            }
        }
    }
}