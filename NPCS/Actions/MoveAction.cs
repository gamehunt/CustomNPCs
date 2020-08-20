using Exiled.API.Features;
using MEC;
using NPCS.Talking;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                case "jump":
                    npc.ReferenceHub.animationController.Networkspeed = new UnityEngine.Vector2(1, 1);
                    npc.NPCComponent.attached_coroutines.Add(Timing.CallDelayed(float.Parse(args["duration"].Replace('.', ',')), () => npc.ReferenceHub.animationController.NetworkcurAnim = 0));
                    break;
                default:
                    npc.Move(Npc.MovementDirection.NONE);
                    break;
            }
            npc.NPCComponent.attached_coroutines.Add(Timing.CallDelayed(float.Parse(args["duration"].Replace('.',',')), () => npc.Move(Npc.MovementDirection.NONE)));
        }
    }
}
