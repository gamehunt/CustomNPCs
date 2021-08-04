using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class FireEventAction : NodeAction
    {
        public override string Name => "FireEventAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            if (bool.Parse(args["global"]))
            {
                foreach (Npc n in Npc.List)
                {
                    n.FireEvent(args["event_name"], new Dictionary<string, object>() { });
                }
            }
            else
            {
                npc.FireEvent(args["event_name"], new Dictionary<string, object>() { });
            }
        }
    }
}