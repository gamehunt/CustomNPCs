using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class DieAction : NodeAction
    {
        public override string Name => "DieAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            npc.PlayerInstance.Kill();
        }
    }
}