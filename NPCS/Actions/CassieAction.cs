using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class CassieAction : NodeAction
    {
        public override string Name => "CassieAction";

        public override bool IsExclusive => false;

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            Cassie.Message(args["message"], bool.Parse(args["held"]), bool.Parse(args["noise"]));
        }
    }
}