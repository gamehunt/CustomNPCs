namespace NPCS.AI
{
    internal class AIFollowTarget : AITarget
    {
        public override string Name => "AIFollowTarget";

        public override string[] RequiredArguments => new string[] { "allow_teleport" };

        private bool allow_tp = true;

        public override bool Check(Npc npc)
        {
            return npc.CurrentAIPlayerTarget != null && (npc.FollowTarget == null || !npc.FollowTarget.IsAlive);
        }

        public override void Construct()
        {
            allow_tp = bool.Parse(Arguments["allow_teleport"]);
        }

        public override float Process(Npc npc)
        {
            npc.DisableFollowAutoTeleport = !allow_tp;
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