using NPCS.Navigation;
using System.Collections.Generic;
using System.Linq;

namespace NPCS.AI
{
    internal class AIFindItemTarget : AITarget
    {
        public override string Name => "AIFindItemTarget";

        public override string[] RequiredArguments => new string[] { "type", "smart" };

        public override bool Check(Npc npc)
        {
            return npc.CurrentAIItemNodeTarget == null && npc.CurrentAIItemGroupTarget == null && (npc.FreeSlots > 0 || smart);
        }

        private Queue<NavigationNode> possible_nodes;
        private bool smart;
        private string type;

        public override float Process(Npc npc)
        {
            if (possible_nodes == null || possible_nodes.Count == 0)
            {
                possible_nodes = new Queue<NavigationNode>(NavigationNode.AllNodes.Values.Where(n => n.PossibleItemTypes.Contains(type)));
                if (smart)
                {
                    if (type == "keycard")
                    {
                        int count = possible_nodes.Count;
                        for (int i = 0; i < count; i++)
                        {
                            NavigationNode node = possible_nodes.Dequeue();
                            if (npc.AvailableKeycards.Length == 0 || (npc.AvailableKeycards.Max() != ItemType.KeycardO5 && node.PossibleItemTypes.Contains((npc.AvailableKeycards.Max() + 1).ToString("g"))))
                            {
                                possible_nodes.Enqueue(node);
                            }
                        }
                    }
                }
            }
            while (possible_nodes.Count > 0)
            {
                NavigationNode node = possible_nodes.Dequeue();
                if (npc.GotoNode(node))
                {
                    Exiled.API.Features.Log.Debug($"Selected item node: {node.Name}", Plugin.Instance.Config.VerboseOutput);
                    npc.CurrentAIItemNodeTarget = node;
                    npc.CurrentAIItemGroupTarget = type;
                    break;
                }
            }
            return 0f;
        }

        protected override AITarget CreateInstance()
        {
            return new AIFindItemTarget();
        }

        public override void Construct()
        {
            smart = bool.Parse(Arguments["smart"]);
            type = Arguments["type"].Trim();
        }
    }
}