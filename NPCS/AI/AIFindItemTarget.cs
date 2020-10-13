using Exiled.API.Extensions;
using MEC;
using System.Linq;
using UnityEngine;

namespace NPCS.AI
{
    internal class AIFindItemTarget : AITarget
    {
        public override string Name => "AIFindItemTarget";

        public override string[] RequiredArguments => new string[] { "range", "type" };

        public override bool Check(Npc npc)
        {
            return npc.CurrentAIItemTarget == null && npc.FreeSlots > 0;
        }

        private bool CheckType(string type, ItemType item)
        {
            switch (type)
            {
                case "keycard":
                    return item.IsKeycard();

                case "weapon":
                    return item.IsWeapon();

                default:
                    return true;
            }
        }

        private float range;

        public override float Process(Npc npc)
        {
            IsFinished = true;
             
            string type = Arguments["type"];
            Pickup pickup = UnityEngine.Object.FindObjectsOfType<Pickup>().Where(p => !p.Locked && !p.InUse && CheckType(type, p.itemId) && Vector3.Distance(npc.NPCPlayer.Position, p.position) < range).FirstOrDefault();
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

        public override void Construct()
        {
            range = float.Parse(Arguments["range"].Replace(".", ","));
        }
    }
}