using HarmonyLib;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(MicroHID), nameof(MicroHID.UpdateServerside))]
    public class MicroHIDFix
    {
        public static bool Prefix(MicroHID __instance)
        {
            return !Npc.Dictionary.ContainsKey(__instance.refHub.gameObject);
        }
    }
}