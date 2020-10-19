using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NPCS.AI
{
    //Resets nav
    internal class AINavigateToRoom : AITarget
    {
        public override string Name => "AINavigateToRoom";

        public override string[] RequiredArguments => new string[] { "safe", "room" };

        public override bool Check(Npc npc)
        {
            return npc.CurrentAIRoomTarget == null && npc.FollowTarget == null;
        }

        private bool safe = true;
        private bool random = false;
        private Room room;

        public override void Construct()
        {
            safe = bool.Parse(Arguments["safe"]);
            random = Arguments["room"].Equals("random", StringComparison.OrdinalIgnoreCase);
            if (!random)
            {
                room = Map.Rooms.Where(rm => rm.Name.Equals(Arguments["room"], StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            }
        }

        public override float Process(Npc npc)
        {
            if (random)
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
            else if (room != null)
            {
                if (npc.GotoRoom(room))
                {
                    npc.CurrentAIRoomTarget = room;
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