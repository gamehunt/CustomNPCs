using Exiled.API.Features;
using NPCS.Talking;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class RotationAction : NodeAction
    {
        public override string Name => "RotationAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            if (bool.Parse(args["absolute"]))
            {
                npc.NPCPlayer.Rotations = new UnityEngine.Vector2(float.Parse(args["x"]), float.Parse(args["y"]));
            }
            else
            {
                npc.NPCPlayer.Rotations += new UnityEngine.Vector2(float.Parse(args["x"]), float.Parse(args["y"]));
            }
        }
    }
}