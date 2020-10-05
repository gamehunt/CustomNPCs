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
            return npc.CurrentAIRoomTarget == null && !string.IsNullOrEmpty(Arguments["room"]) && (Arguments["room"].Equals("random", StringComparison.OrdinalIgnoreCase) || Map.Rooms.Where(r => r.Name.Equals(Arguments["room"], StringComparison.OrdinalIgnoreCase)).FirstOrDefault() != null);
        }

        public override float Process(Npc npc)
        {
            Log.Debug($"[AI] Processing room selection....", Plugin.Instance.Config.VerboseOutput);
            if (Arguments["room"].Equals("random", StringComparison.OrdinalIgnoreCase))
            {
                Room r = Map.Rooms[Plugin.Random.Next(0, Map.Rooms.Count)];
                Log.Debug($"[AI] Room selected: {r.Name}", Plugin.Instance.Config.VerboseOutput);
                if (npc.GotoRoom(r))
                {
                    npc.CurrentAIRoomTarget = r;
                }
            }
            else
            {
                Room r = Map.Rooms.Where(rm => rm.Name.Equals(Arguments["room"], StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (npc.GotoRoom(r))
                {
                    npc.CurrentAIRoomTarget = r;
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