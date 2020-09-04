using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Conditions
{
    internal class RandomCondition : NodeCondition
    {
        public override string Name => "RandomCondition";

        public override bool Check(Player player, Dictionary<string, string> args)
        {
            return Plugin.Random.Next(0, 101) < int.Parse(args["chance"]);
        }
    }
}