using Exiled.API.Features;
using NPCS.Talking;
using System;
using System.Collections.Generic;

namespace NPCS.Conditions
{
    class RandomCondition : NodeCondition
    {
        public override string Name => "RandomCondition";

        public override bool Check(Player player, Dictionary<string, string> args)
        {
            return RandomGenerator.GetInt16(0, 101) < int.Parse(args["chance"]);
        }
    }
}
