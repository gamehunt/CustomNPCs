using HarmonyLib;
using PlayableScps;
using UnityEngine;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.GetVisionInformation))]
    internal class Scp096Patch
    {
        private static bool Prefix(Scp096 __instance, ref VisionInformation __result, GameObject source)
        {
            NPCS.Npc npc = NPCS.Npc.Dictionary.ContainsKey(source) ? NPCS.Npc.Dictionary[source] : null;
            VisionInformation visionInformation = new VisionInformation
            {
                Source = source,
                Target = __instance.Hub.gameObject,
                RaycastHit = false,
                Looking = false
            };
            if (npc != null && !npc.ShouldTrigger096)
            {
                __result = visionInformation;
                return false;
            }
            return true;
        }
    }
}
