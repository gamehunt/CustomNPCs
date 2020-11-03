using System;

namespace NPCS.AI
{
    internal class AIFollowTarget : AITarget
    {
        public override string Name => "AIFollowTarget";

        public override string[] RequiredArguments => new string[] { "target_lost_behaviour" };

        private Npc.TargetLostBehaviour behav = Npc.TargetLostBehaviour.TELEPORT;

        public override bool Check(Npc npc)
        {
            return npc.CurrentAIPlayerTarget != null && (npc.FollowTarget == null || !npc.FollowTarget.IsAlive);
        }

        public override void Construct()
        {
            behav = (Npc.TargetLostBehaviour)Enum.Parse(typeof(Npc.TargetLostBehaviour),Arguments["target_lost_behaviour"]);
        }

        public override float Process(Npc npc)
        {
            npc.OnTargetLostBehaviour = behav;
            npc.Stop();
            npc.Follow(npc.CurrentAIPlayerTarget);
            IsFinished = true;
            return 0f;
        }

        protected override AITarget CreateInstance()
        {
            return new AIFollowTarget();
        }
    }
}