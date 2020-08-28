using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NPCS.AI
{
    class AIFindTarget : AITarget
    {
        public override string Name => "AIFindTarget";

        public override bool Check(Npc npc)
        {
            return true;
        }

        public override float Process(Npc npc)
        {
            IsFinished = true;
            float range = float.Parse(Arguments["range"].Replace(".", ","));
            bool enemy = bool.Parse(Arguments["enemy"].Replace(".", ","));
            foreach (Player p in Player.List.Where(pl => (pl.Side != npc.NPCPlayer.Side || !enemy)))
            {
                if(Vector3.Distance(p.Position,npc.NPCPlayer.Position) < range && !Physics.Linecast(npc.NPCPlayer.Position, p.Position, npc.NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces))
                {
                    npc.CurrentAIPlayerTarget = p;
                    return 0f;
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
