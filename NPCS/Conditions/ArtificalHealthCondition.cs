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
            float value = float.Parse(args["value"].Replace('.', ','));
            return Utils.Utils.CompareWithType(args["comparsion_type"], player.AdrenalineHealth, value);
        }
    }
}