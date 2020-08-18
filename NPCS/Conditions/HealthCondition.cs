using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Conditions
{
    internal class HealthCondition : NodeCondition
    {
        public override string Name => "HealthCondition";

        public override bool Check(Player player, Dictionary<string, string> args)
        {
            switch (args["comparsion_type"])
            {
                case "equals":
                    return player.Health.Equals(float.Parse(args["value"]));

                case "greater":
                    return player.Health > float.Parse(args["value"]);

                case "less":
                    return player.Health < float.Parse(args["value"]);

                case "greater_or_equals":
                    return player.Health >= float.Parse(args["value"]);

                case "less_or_equals":
                    return player.Health <= float.Parse(args["value"]);

                case "not_equals":
                    return !player.Health.Equals(float.Parse(args["value"]));

                default:
                    return false;
            }
        }
    }
}