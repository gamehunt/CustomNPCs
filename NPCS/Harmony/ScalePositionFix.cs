﻿using HarmonyLib;
using UnityEngine;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.OverridePosition))]
    [HarmonyPriority(Priority.First)]
    internal class ScalePositionFix
    {
        private static bool Prefix(PlayerMovementSync __instance, Vector3 pos, float rot, bool forceGround)
        {
            if (!Npc.Dictionary.ContainsKey(__instance.gameObject))
            {
                return true;
            }
            RaycastHit raycastHit;
            if (forceGround && Physics.Raycast(pos, Vector3.down, out raycastHit, 100f, __instance.CollidableSurfaces))
            {
                pos = raycastHit.point + Vector3.up * Plugin.Instance.Config.NpcSizePositionMultiplier;
                pos = new Vector3(pos.x, pos.y - (1f - __instance._hub.transform.localScale.y) * Plugin.Instance.Config.NpcSizePositionMultiplier, pos.z);
            }
            __instance.ForcePosition(pos);
            __instance.PlayScp173SoundIfTeleported();
            return false;
        }
    }
}