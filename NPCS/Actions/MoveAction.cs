using Exiled.API.Features;
using MEC;
using NPCS.Talking;
using System;
using System.Collections.Generic;
using System.Text;

namespace NPCS.Actions
{
    class MoveAction : NodeAction
    {
        public override string Name => "MoveAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            switch (args["direction"])
            {
                case "forward":
                    npc.Move(Npc.MovementDirection.FORWARD);
                    break;
                case "backward":
                    npc.Move(Npc.MovementDirection.BACKWARD);
                    break;
                case "right":
                    npc.Move(Npc.MovementDirection.RIGHT);
                    break;
                case "left":
                    npc.Move(Npc.MovementDirection.LEFT);
                    break;
                default:
                    npc.Move(Npc.MovementDirection.NONE);
                    break;
            }
            Timing.CallDelayed(float.Parse(args["duration"]), () => npc.Move(Npc.MovementDirection.NONE));
        }
    }
}
