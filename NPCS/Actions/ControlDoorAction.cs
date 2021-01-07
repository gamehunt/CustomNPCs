using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using Interactables.Interobjects.DoorUtils;
using Exiled.API.Extensions;

namespace NPCS.Actions
{
    internal class ControlDoorAction : Talking.NodeAction
    {
        public override string Name => "ControlDoorAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            DoorVariant d = null;
            if (DoorNametagExtension.NamedDoors.ContainsKey(args["door"]))
            {
                d = DoorNametagExtension.NamedDoors[args["door"]].TargetDoor;
            }
            if (d != null)
            {
                switch (args["type"])
                {
                    case "lock":
                        d.ServerChangeLock(DoorLockReason.AdminCommand, true);
                        break;

                    case "ulock":
                        d.ServerChangeLock(DoorLockReason.AdminCommand, false);
                        break;

                    case "open":
                        d.NetworkTargetState = true;
                        break;

                    case "close":
                        d.NetworkTargetState = false;
                        break;

                    case "destroy":
                        d.BreakDoor();
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