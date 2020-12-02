using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using NPCS.Navigation;

namespace NPCS.AI
{
    //WIP
    class AIFindAmmoTarget : AITarget
    {
        public override string Name => "AIFindAmmoTarget";

        public override string[] RequiredArguments => new string[] { "type" };

        private AmmoType target_type;
        private string string_type;
        bool IsAuto = false;
        private Queue<NavigationNode> possible_nodes;

        private ItemType ItemFromAmmo(AmmoType type)
        {
            switch (type) {
                case AmmoType.Nato556:
                    return ItemType.Ammo556;
                case AmmoType.Nato762:
                    return ItemType.Ammo762;
                case AmmoType.Nato9:
                    return ItemType.Ammo9mm;
                default:
                    return ItemType.None;
            }
        }

        public override bool Check(Npc npc)
        {
            uint limit;
            if (!IsAuto)
            {
                limit = npc.NPCPlayer.ReferenceHub.searchCoordinator.ConfigPipe.GetLimitAmmo((byte)target_type);
                return npc.NPCPlayer.Ammo[(int)target_type] < limit;
            }
            else
            {
                return true;
            }
        }

        public override void Construct()
        {
            if (Arguments["type"].Equals("auto"))
            {
                IsAuto = true;
            }
            else
            {
                target_type = (AmmoType)Enum.Parse(typeof(AmmoType), Arguments["type"]);
                string_type = ItemFromAmmo(target_type).ToString("g");
            }
        }

        public override float Process(Npc npc)
        {
            if (IsAuto)
            {
                uint limit = npc.NPCPlayer.ReferenceHub.searchCoordinator.ConfigPipe.GetLimitAmmo((byte)AmmoType.Nato556);
                if (npc.NPCPlayer.Ammo[(int)AmmoType.Nato556] < limit)
                {
                    target_type = AmmoType.Nato556;
                }
                else
                {
                    limit = npc.NPCPlayer.ReferenceHub.searchCoordinator.ConfigPipe.GetLimitAmmo((byte)AmmoType.Nato762);
                    if (npc.NPCPlayer.Ammo[(int)AmmoType.Nato762] < limit)
                    {
                        target_type = AmmoType.Nato762;
                    }
                    else
                    {
                        limit = npc.NPCPlayer.ReferenceHub.searchCoordinator.ConfigPipe.GetLimitAmmo((byte)AmmoType.Nato9);
                        if (npc.NPCPlayer.Ammo[(int)AmmoType.Nato9] < limit)
                        {
                            target_type = AmmoType.Nato9;
                        }
                        else
                        {
                            return 0f;
                        }
                    }
                }
                string_type = ItemFromAmmo(target_type).ToString("g");
            }
            if (IsAuto || possible_nodes == null || possible_nodes.Count == 0)
            {
                possible_nodes = new Queue<NavigationNode>(NavigationNode.AllNodes.Values.Where(n => n.PossibleItemTypes.Contains(string_type)));
            }
            while (possible_nodes.Count > 0)
            {
                NavigationNode node = possible_nodes.Dequeue();
                if (npc.GotoNode(node))
                {
                    Exiled.API.Features.Log.Debug($"Selected item node: {node.Name}", Plugin.Instance.Config.VerboseOutput);
                    npc.CurrentAIItemNodeTarget = node;
                    npc.CurrentAIItemGroupTarget = string_type;
                    break;
                }
            }
            return 0f;
        }

        protected override AITarget CreateInstance()
        {
            return new AIFindAmmoTarget();
        }
    }
}
