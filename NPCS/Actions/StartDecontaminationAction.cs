using Exiled.API.Features;
using LightContainmentZoneDecontamination;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class StartDecontaminationAction : Talking.NodeAction
    {
        public override string Name => "StartDecontaminationAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            DecontaminationController.Singleton.FinishDecontamination();
        }
    }
}