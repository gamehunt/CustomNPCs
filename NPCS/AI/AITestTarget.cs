using Exiled.API.Features;

namespace NPCS.AI
{
    internal class AITestTarget : AITarget
    {
        public override string Name => "AITestTarget";

        public override bool Check(Npc npc)
        {
            return true;
        }

        public override float Process(Npc npc)
        {
            Log.Info("PROCESSING AI");
            return 0.1f;
        }

        protected override AITarget CreateInstance()
        {
            return new AITestTarget();
        }
    }
}