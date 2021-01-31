using Exiled.API.Features;
using HarmonyLib;
using MEC;
using System.Linq;
using UnityEngine;

namespace ReversePlagueCompatModule.Harmony
{
    [HarmonyPatch(typeof(ReversePlague.EventHandlers), nameof(ReversePlague.EventHandlers.OnPlayerDie))]
    internal class OnDiedPatch
    {
        private static bool Prefix(ReversePlague.EventHandlers __instance, Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (!ev.HitInformation.IsPlayer)
            {
                return false;
            }
            Team team = ev.Target.Team;
            if (!__instance.infectable.Contains(ev.Target.ReferenceHub))
            {
                return false;
            }
            __instance.infectable.RemoveAll((ReferenceHub p) => p == ev.Target.ReferenceHub);
            foreach (Player player2 in from x in Player.List
                                       where x.Role == RoleType.Scp049 && !x.IsNPC()
                                       select x)
            {
                ReferenceHub referenceHub = player2.ReferenceHub;
                if (Vector3.Distance(ev.Target.GameObject.transform.position, referenceHub.transform.position) < RPPlugin.plugin.Config.range)
                {
                    foreach (Inventory.SyncItemInfo syncItemInfo in ev.Target.ReferenceHub.inventory.items)
                    {
                        ev.Target.ReferenceHub.inventory.SetPickup(syncItemInfo.id, syncItemInfo.durability, ev.Target.GameObject.transform.position, ev.Target.GameObject.transform.rotation, 0, 0, 0);
                    }
                    ReferenceHub player = ev.Target.ReferenceHub;
                    Vector3 pos = player.transform.position;
                    ev.Target.ReferenceHub.characterClassManager.SetPlayersClass(RoleType.Scp0492, ev.Target.GameObject, false, false);
                    Timing.CallDelayed(RPPlugin.plugin.Config.teleportDelay, delegate ()
                    {
                        player.playerMovementSync.OverridePosition(pos, 0f, false);
                    });
                }
            }
            return false;
        }
    }
}