using Exiled.API.Features;
using NPCS.Events;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class FireEventAction : NodeAction
    {
        public override string Name => "FireEventAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            NPCCustomEvent ev = new NPCCustomEvent(npc, player, args["event_name"]);
            if (bool.Parse(args["global"]))
            {
                foreach(Npc n in Npc.List)
                {
                    n.FireEvent(ev);
                }
            }
            else
            {
                npc.FireEvent(ev);
            }
        }
    }
}