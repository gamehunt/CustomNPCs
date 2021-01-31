namespace NPCS.AI
{
    internal class AIStop : AITarget
    {
        public override string Name => "AIStop";

        public override string[] RequiredArguments => new string[] { };

        public override bool Check(Npc npc)
        {
            return true;
        }

        public override void Construct()
        {
        }

        public override float Process(Npc npc)
        {
            npc.AIEnabled = false;

            return 0f;
        }

        protected override AITarget CreateInstance()
        {
            return new AIStop();
        }
    }
}