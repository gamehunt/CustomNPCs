using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;
using System.Linq;

namespace NPCS.Conditions
{
    internal class RoleExistsCondition : NodeCondition
    {
        public override string Name => "RoleExistsCondition";

        public override bool Check(Player player, Dictionary<string, string> args)
        {
            bool need_npcs = bool.Parse(args["need_npcs"]);
            return !Player.Get((RoleType)int.Parse(args["role"])).Where(p => p.GameObject.GetComponent<Npc>() == null || need_npcs).IsEmpty(); //I hate lambdas...
        }
    }
}