using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Conditions
{
    internal class ArtificalHealthCondition : NodeCondition
    {
        public override string Name => "ArtificalHealthCondition";

        public override bool Check(Player player, Dictionary<string, string> args)
        {
            switch (args["comparsion_type"])
            {
                case "equals":
                    return player.AdrenalineHealth.Equals(float.Parse(args["value"]));

                case "greater":
                    return player.AdrenalineHealth > float.Parse(args["value"]);

                case "less":
                    return player.AdrenalineHealth < float.Parse(args["value"]);

                case "greater_or_equals":
                    return player.AdrenalineHealth >= float.Parse(args["value"]);

                case "less_or_equals":
                    return player.AdrenalineHealth <= float.Parse(args["value"]);

                case "not_equals":
                    return !player.AdrenalineHealth.Equals(float.Parse(args["value"]));

                default:
                    return false;
            }
        }
    }
}