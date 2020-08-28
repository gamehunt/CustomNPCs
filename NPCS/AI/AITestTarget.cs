using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Text;

namespace NPCS.AI
{
    class AITestTarget : AITarget
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
