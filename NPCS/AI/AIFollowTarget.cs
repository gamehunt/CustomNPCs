using Exiled.API.Features;

namespace NPCS.AI
{
    internal class AIFollowTarget : AITarget
    {
        public override string Name => "AIFollowTarget";

        public override bool Check(Npc npc)
        {
            return npc.CurrentAIPlayerTarget != null && Player.Dictionary.ContainsKey(npc.CurrentAIPlayerTarget.GameObject);
        }

        public override void Contruct()
        {
            
        }

        public override float Process(Npc npc)
        {
            npc.DisableFollowAutoTeleport = false;
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