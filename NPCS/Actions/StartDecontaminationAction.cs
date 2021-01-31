using Exiled.API.Features;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class StartDecontaminationAction : Talking.NodeAction
    {
        public override string Name => "StartDecontaminationAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            Map.StartDecontamination();
        }
    }
}