using Exiled.API.Features;
using Exiled.API.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;

namespace NPCS.AI
{
    //Resets nav
    internal class AINavigateToRoom : AITarget
    {
        public override string Name => "AINavigateToRoom";

        public override bool Check(Npc npc)
        {
            return npc.CurrentAIRoomTarget == null && npc.FollowTarget == null && !string.IsNullOrEmpty(Arguments["room"]) && !string.IsNullOrEmpty(Arguments["safe"]) &&  (Arguments["room"].Equals("random", StringComparison.OrdinalIgnoreCase) || Map.Rooms.Where(r => r.Name.RemoveBracketsOnEndOfName().Equals(Arguments["room"], StringComparison.OrdinalIgnoreCase)).FirstOrDefault() != null);
        }

        public override float Process(Npc npc)
        {
            bool safe = bool.Parse(Arguments["safe"]);
            if (Arguments["room"].Equals("random", StringComparison.OrdinalIgnoreCase))
            {
                List<Room> valid_rooms = Map.Rooms.Where(rm => rm.Zone != Exiled.API.Enums.ZoneType.LightContainment || (safe ? Round.ElapsedTime.Minutes < 10 : !Map.IsLCZDecontaminated)).ToList();
                Room r = valid_rooms[Plugin.Random.Next(0, valid_rooms.Count)];
                Log.Debug($"[AI] Room selected: {r.Name}", Plugin.Instance.Config.VerboseOutput);
                npc.Stop();
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