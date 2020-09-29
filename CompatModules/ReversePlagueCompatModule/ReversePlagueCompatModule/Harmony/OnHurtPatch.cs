using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using NPCS;
using ReversePlague;
using System.Collections.Generic;
using UnityEngine;

namespace ReversePlagueCompatModule.Harmony
{
    [HarmonyPatch(typeof(ReversePlague.EventHandlers),nameof(ReversePlague.EventHandlers.OnPlayerHurt))]
    class OnHurtPatch
    {
        private static bool Prefix(ReversePlague.EventHandlers __instance,Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.Target.IsNPC())
            {
                return false;
            }
			Team team = ev.Target.Team;
			if (team == Team.SCP || team == Team.RIP || (team == Team.TUT && !RPPlugin.plugin.Config.tutorialInfect))
			{
				return false;
			}
			if (!__instance.infectable.Contains(ev.Target.ReferenceHub))
			{
				__instance.infectable.Add(ev.Target.ReferenceHub);
			}
			return false;
        }
    }
}
