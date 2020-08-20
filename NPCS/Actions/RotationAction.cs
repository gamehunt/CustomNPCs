using Exiled.API.Features;
using NPCS.Talking;
using System;
using System.Collections.Generic;
using System.Text;

namespace NPCS.Actions
{
    class RotationAction : NodeAction
    {
        public override string Name => "RotationAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            if (bool.Parse(args["absolute"]))
            {
                npc.Rotation = new UnityEngine.Vector2(float.Parse(args["x"]), float.Parse(args["y"]));
            }
            else
            {
                npc.Rotation += new UnityEngine.Vector2(float.Parse(args["x"]), float.Parse(args["y"]));
            }
        }
    }
}
