using Exiled.API.Features;
using MEC;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class MoveAction : NodeAction
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
                    npc.NPCPlayer.ReferenceHub.animationController.Networkspeed = new UnityEngine.Vector2(1, 1);
                    npc.AttachedCoroutines.Add(Timing.CallDelayed(float.Parse(args["duration"].Replace('.', ',')), () => npc.NPCPlayer.ReferenceHub.animationController.NetworkcurAnim = 0));
                    break;

                default:
                    npc.Move(Npc.MovementDirection.NONE);
                    break;
            }
            npc.MovementCoroutines.Add(Timing.CallDelayed(float.Parse(args["duration"].Replace('.', ',')), () => npc.Move(Npc.MovementDirection.NONE)));
        }
    }
}