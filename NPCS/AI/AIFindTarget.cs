using Exiled.API.Features;
using NPCS.AI.TargetFilters;
using System.Linq;
using UnityEngine;

namespace NPCS.AI
{
    internal class AIFindTarget : AITarget
    {
        public override string Name => "AIFindTarget";

        private static readonly Scp939TargetFilter scp939_filter = new Scp939TargetFilter(); //TODO make registry for target filters

        public override bool Check(Npc npc)
        {
            return true;
        }

        public override float Process(Npc npc)
        {
            IsFinished = true;
            float range = float.Parse(Arguments["range"].Replace(".", ","));
            bool enemy = bool.Parse(Arguments["enemy"].Replace(".", ","));
            string target_filter = Arguments["filter"];

            foreach (Player p in Player.List.Where(pl => (pl.Side != npc.NPCPlayer.Side || !enemy)))
            {
                if (Vector3.Distance(p.Position, npc.NPCPlayer.Position) < range && !Physics.Linecast(npc.NPCPlayer.Position, p.Position, npc.NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces))
                {
                    bool res = true;
                    if (target_filter == "scp939")
                    {
                        res = scp939_filter.Check(npc, p);
                    }
                    if (res)
                    {
                        npc.CurrentAIPlayerTarget = p;
                        return 0f;
                    }
                }
            }
            npc.CurrentAIPlayerTarget = null;
            return 0f;
        }

        protected override AITarget CreateInstance()
        {
            return new AIFindTarget();
        }
    }
}