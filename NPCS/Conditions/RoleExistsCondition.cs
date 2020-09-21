using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;
using System.Linq;
using System;

namespace NPCS.Conditions
{
    internal class RoleExistsCondition : NodeCondition
    {
        public override string Name => "RoleExistsCondition";

        public override bool Check(Player player, Dictionary<string, string> args)
        {
            bool need_npcs = bool.Parse(args["need_npcs"]);
            return !Player.Get((RoleType)Enum.Parse(typeof(RoleType),args["role"])).Where(p => !Npc.Dictionary.ContainsKey(p.GameObject) || need_npcs).IsEmpty();
        }
    }
}