using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NPCS.AI
{
    class AIFindItemTarget : AITarget
    {
        public override string Name => "AIFindItemTarget";

        public override bool Check(Npc npc)
        {
            return npc.CurrentAIItemTarget == null;
        }

        public override float Process(Npc npc)
        {
            IsFinished = true;
            float range = float.Parse(Arguments["range"].Replace(".", ","));
            Pickup pickup = UnityEngine.Object.FindObjectsOfType<Pickup>().Where(p => !p.Locked && Vector3.Distance(npc.NPCPlayer.Position, p.position) < range).FirstOrDefault();
            if(pickup != null)
            {
                npc.Stop();
                npc.CurrentAIItemTarget = pickup;
                npc.GoTo(pickup.position);
            }
            return 0f;
        }

        protected override AITarget CreateInstance()
        {
            return new AIFindItemTarget();
        }
    }
}
