using Exiled.API.Features;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class ChangeRoleAction : Talking.NodeAction
    {
        public override string Name => "ChangeRoleAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            if (bool.Parse(args["preserve_position"]))
            {
                player.ReferenceHub.characterClassManager.NetworkCurClass = (RoleType)int.Parse(args["role"]);
            }
            else
            {
                player.Role = (RoleType)int.Parse(args["role"]);
            }
        }
    }
}