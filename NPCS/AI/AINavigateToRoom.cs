using Exiled.API.Features;
using System;
using System.Linq;

namespace NPCS.AI
{
    internal class AINavigateToRoom : AITarget
    {
        public override string Name => "AINavigateToRoom";

        public override bool Check(Npc npc)
        {
            return Arguments["room"].Equals("random", StringComparison.OrdinalIgnoreCase) || Map.Rooms.Where(r => r.Name.Equals(Arguments["room"], StringComparison.OrdinalIgnoreCase)).FirstOrDefault() != null;
        }

        public override float Process(Npc npc)
        {
            if (Arguments["room"].Equals("random", StringComparison.OrdinalIgnoreCase))
            {
                Room r = Map.Rooms[RandomGenerator.GetInt32(true) % Map.Rooms.Count];
                if (r != npc.CurrentAIRoomTarget)
                {
                    if (npc.GotoRoom(r))
                    {
                        npc.CurrentAIRoomTarget = r;
                    }
                }
            }
            else
            {
                Room r = Map.Rooms.Where(rm => rm.Name.Equals(Arguments["room"], StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (r != npc.CurrentAIRoomTarget)
                {
                    if (npc.GotoRoom(r))
                    {
                        npc.CurrentAIRoomTarget = r;
                    }
                }
            }
            IsFinished = true;
            return 0f;
        }

        protected override AITarget CreateInstance()
        {
            return new AINavigateToRoom();
        }
    }
}