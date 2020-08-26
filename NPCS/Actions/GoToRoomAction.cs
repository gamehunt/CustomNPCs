using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPCS.Actions
{
    class GoToRoomAction : Talking.NodeAction
    {
        public override string Name => "GoToRoomAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            Room r = Map.Rooms.Where(room => room.Name.Equals(args["room"], StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if(r != null)
            {
                npc.GotoRoom(r);
            }
            else
            {
                Log.Error($"Failed to find room {args["room"]}!");
            }
        }
    }
}
