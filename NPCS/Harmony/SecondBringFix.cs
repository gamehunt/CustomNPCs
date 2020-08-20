using HarmonyLib;
using UnityEngine;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.OverridePosition))]
    [HarmonyPriority(Priority.First)]
    internal class SecondBringFix
    {
        private static bool Prefix(PlayerMovementSync __instance, Vector3 pos, float rot, bool forceGround)
        {
            if (__instance.gameObject.GetComponent<NPCComponent>() == null)
            {
                return true;
            }
            RaycastHit raycastHit;
            if (forceGround && Physics.Raycast(pos, Vector3.down, out raycastHit, 100f, __instance.CollidableSurfaces))
            {
                pos = raycastHit.point + Vector3.up * 1.23f;
            }
            __instance.ForcePosition(pos);
            __instance.PlayScp173SoundIfTeleported();
            return false;
        }
    }
}