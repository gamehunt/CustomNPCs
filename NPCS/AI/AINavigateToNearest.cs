using System;
using System.Collections.Generic;
using System.Text;

namespace NPCS.AI
{
    class AINavigateToNearest : AITarget
    {
        public override string Name => throw new NotImplementedException();

        public override bool Check(Npc npc)
        {
            throw new NotImplementedException();
        }

        public override float Process(Npc npc)
        {
            throw new NotImplementedException();
        }

        protected override AITarget CreateInstance()
        {
            return new AINavigateToNearest();
        }
    }
}
