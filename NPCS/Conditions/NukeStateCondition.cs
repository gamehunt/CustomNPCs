using Exiled.API.Features;
using NPCS.Talking;
using System;
using System.Collections.Generic;

namespace NPCS.Conditions
{
    internal class NukeStateCondition : NodeCondition
    {
        public override string Name => "NukeStateCondition";

        public override bool Check(Player player, Dictionary<string, string> args)
        {
            switch (args["state"])
            {
                case "idle":
                    return !Warhead.IsDetonated && !Warhead.IsInProgress;
                case "not_detonated":
                    return !Warhead.IsDetonated;
                case "not_in_progress":
                    return !Warhead.IsInProgress;
                case "in_progress":
                    return Warhead.IsInProgress;
                case "detonated":
                    return Warhead.IsDetonated;
                case "not_idle":
                    return Warhead.IsInProgress || Warhead.IsDetonated;
                default:
                    return false;
            }
        }
    }
}