using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Conditions
{
    internal class PermissionCondition : NodeCondition
    {
        public override string Name => "PermissionCondition";

        public override bool Check(Player player, Dictionary<string, string> args)
        {
            return player.CheckPermission(args["permission"]);
        }
    }
}