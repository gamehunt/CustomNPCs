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
            float value = float.Parse(args["value"].Replace('.', ','));
            return Utils.Utils.CompareWithType(args["comparsion_type"], player.Health, value);
        }
    }
}