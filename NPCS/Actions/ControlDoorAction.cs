using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NPCS.Actions
{
    internal class ControlDoorAction : Talking.NodeAction
    {
        public override string Name => "ControlDoorAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            Door d = Map.Doors.Where(dr => dr.DoorName.Equals(args["door"], StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (d != null)
            {
                switch (args["type"])
                {
                    case "lock":
                        d.Networklocked = true;
                        break;

                    case "ulock":
                        d.Networklocked = false;
                        break;

                    case "open":
                        d.NetworkisOpen = true;
                        break;

                    case "close":
                        d.NetworkisOpen = false;
                        break;

                    case "destroy":
                        d.Networkdestroyed = true;
                        break;

                    default:
                        Log.Error($"Unknown door ctrl action {args["type"]}!");
                        break;
                }
            }
            else
            {
                Log.Error($"Can't find door {args["door"]}!");
            }
        }
    }
}