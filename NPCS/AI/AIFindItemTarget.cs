using MEC;
using System.Linq;
using UnityEngine;

namespace NPCS.AI
{
    internal class AIFindItemTarget : AITarget
    {
        public override string Name => "AIFindItemTarget";

        public override bool Check(Npc npc)
        {
            return npc.CurrentAIItemTarget == null && npc.FreeSlots > 0;
        }

        public override float Process(Npc npc)
        {
            IsFinished = true;
            float range = float.Parse(Arguments["range"].Replace(".", ","));
            Pickup pickup = UnityEngine.Object.FindObjectsOfType<Pickup>().Where(p => !p.Locked && !p.InUse && Vector3.Distance(npc.NPCPlayer.Position, p.position) < range).FirstOrDefault();
            if (pickup != null)
            {
                npc.Stop();
                npc.CurrentAIItemTarget = pickup;
                npc.MovementCoroutines.Add(Timing.CallDelayed(npc.GoTo(pickup.position), () =>
                {
                    if (npc.CurrentAIItemTarget != null)
                    {
                        npc.TakeItem(npc.CurrentAIItemTarget);
                        npc.CurrentAIItemTarget = null;
                    }
                }));
            }
            return 0f;
        }

        protected override AITarget CreateInstance()
        {
            return new AIFindItemTarget();
        }
    }
}