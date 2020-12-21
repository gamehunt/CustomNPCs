using Exiled.API.Features;
using HarmonyLib;
using NPCS;
using Stalky106;
using System.Collections.Generic;
using UnityEngine;

namespace Stalky106CompatModule.Harmony
{
    [HarmonyPatch(typeof(Stalky106.StalkyMethods), nameof(Stalky106.StalkyMethods.FindTarget))]
    internal class StalkCoroutinePatch
    {
        private static bool Prefix(StalkyMethods __instance, ref Player __result, List<Player> validPlayerList, LayerMask teleportPlacementMask, out Vector3 portalPosition)
        {
            __instance.stalky106LastTime = Time.time;
            Player player;
            do
            {
                int index = UnityEngine.Random.Range(0, validPlayerList.Count);
                player = validPlayerList[index];
                RaycastHit raycastHit;
                Physics.Raycast(new Ray(player.GameObject.transform.position, -Vector3.up), out raycastHit, 10f, teleportPlacementMask);
                portalPosition = raycastHit.point;
                validPlayerList.RemoveAt(index);
            }
            while ((player.IsNPC() || portalPosition.Equals(Vector3.zero) || Vector3.Distance(portalPosition, StalkyPlugin.pocketDimension) < 40f) && validPlayerList.Count > 0);
            __result = player.IsNPC() ? null : player;
            return false;
        }
    }
}