using Exiled.API.Features;
using System;
using System.Collections.Generic;

namespace NPCS.Actions
{
    internal class NukeStartAction : Talking.NodeAction
    {
        public override string Name => "NukeStartAction";

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            if (bool.Parse(args["instant"]))
            {
                Warhead.Detonate();
            }
            else
            {
                Warhead.Start();
            }
            Warhead.IsLocked = bool.Parse(args["lock"]);
        }
    }
}