using Exiled.API.Features;
using NPCS.Talking;
using System;
using System.Collections.Generic;

namespace NPCS.Conditions
{
    internal class RoundTimeCondition : NodeCondition
    {
        public override string Name => "RoundTimeCondition";

        public override bool Check(Player player, Dictionary<string, string> args)
        {
            switch (args["comparsion_type"])
            {
                case "equals":
                    return Round.ElapsedTime.TotalSeconds.Equals(float.Parse(args["value"]));

                case "greater":
                    return Round.ElapsedTime.TotalSeconds > float.Parse(args["value"]);

                case "less":
                    return Round.ElapsedTime.TotalSeconds < float.Parse(args["value"]);

                case "greater_or_equals":
                    return Round.ElapsedTime.TotalSeconds >= float.Parse(args["value"]);

                case "less_or_equals":
                    return Round.ElapsedTime.TotalSeconds <= float.Parse(args["value"]);

                case "not_equals":
                    return !Round.ElapsedTime.TotalSeconds.Equals(float.Parse(args["value"]));

                default:
                    return false;
            }
        }
    }
}