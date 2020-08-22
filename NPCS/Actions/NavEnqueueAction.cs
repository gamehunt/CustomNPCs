using Exiled.API.Features;
using NPCS.Navigation;
using NPCS.Talking;
using System;
using System.Collections.Generic;
using System.Text;

namespace NPCS.Actions
{
    class NavEnqueueAction : NodeAction
    {
        public override string Name => "NavEnqueueAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            NavigationNode node = NavigationNode.Get(args["node"]);
            if (node != null)
            {
                bool force = bool.Parse(args["force"]);
                if (force)
                {
                    npc.GoTo(node.Position);
                }
                else
                {
                    npc.AddNavTarget(node);
                }
            }
        }
    }
}
