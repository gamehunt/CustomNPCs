using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using NPCS.Talking;
using System;
using System.Collections.Generic;
using System.Text;

namespace NPCS.Conditions
{
    class PermissionCondition : NodeCondition
    {
        public override string Name => "PermissionCondition";

        public override bool Check(Player player, Dictionary<string, string> args)
        {
            return player.CheckPermission(args["permission"]);
        }
    }
}
