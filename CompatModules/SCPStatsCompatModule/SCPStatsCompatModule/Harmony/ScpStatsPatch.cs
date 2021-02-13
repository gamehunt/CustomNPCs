using Exiled.API.Features;
using HarmonyLib;
using SCPStats.Hats;
using System.Collections.Generic;
using UnityEngine;
using NPCS;

namespace SCPStatsCompatModule.Harmony
{
    [HarmonyPatch(typeof(HatPlayerComponent), nameof(HatPlayerComponent.UpdatePickupPositionForPlayer))]
    public class ScpStatsPatch
    {
        private static bool Prefix(Player player, Pickup pickup, Vector3 position)
        {
            return !player.IsNPC();
        }
    }
}