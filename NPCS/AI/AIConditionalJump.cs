namespace NPCS.AI
{
    internal class AIConditionalJump : AITarget
    {
        public override string Name => "AIConditionalJump";

        public override string[] RequiredArguments => new string[] { "offset", "conditions" };

        private int offset;
        private string[] conditions;

        private bool CheckCondition(Npc npc, string cond)
        {
            bool negate = false;
            if (cond.StartsWith("!"))
            {
                cond = cond.Substring(1);
                negate = true;
            }
            bool res = false;
            switch (cond)
            {
                case "has_player_target":
                    res = npc.CurrentAIPlayerTarget != null;
                    break;

                case "has_room_target":
                    res = npc.CurrentAIRoomTarget != null;
                    break;

                case "has_item_target":
                    res = npc.CurrentAIItemGroupTarget != null;
                    break;

                case "has_follow_target":
                    res = npc.FollowTarget != null;
                    break;

                case "has_nav_queue":
                    res = npc.NavigationQueue.Count != 0;
                    break;

                case "has_weapon":
                    res = npc.AvailableWeapons.Count != 0;
                    break;

                case "has_keycard":
                    res = npc.AvailableKeycards.Length != 0;
                    break;
            }
            return negate ? !res : res;
        }

        public override bool Check(Npc npc)
        {
            foreach (string cond in conditions)
            {
                if (!CheckCondition(npc, cond.Trim()))
                {
                    return false;
                }
            }
            return true;
        }

        public override void Construct()
        {
            offset = int.Parse(Arguments["offset"]);
            conditions = Arguments["conditions"].Split(' ');
        }

        public override float Process(Npc npc)
        {
            IsFinished = true;
            npc.SkippedTargets = offset;
            return 0f;
        }

        protected override AITarget CreateInstance()
        {
            return new AIConditionalJump();
        }
    }
}