using HarmonyLib;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(VersionCheck), nameof(VersionCheck.Start))]
    internal class VersionCheckFix
    {
        private static bool Prefix(VersionCheck __instance)
        {
            if (__instance.connectionToClient == null)
            {
                return false;
            }
            return true;
        }
    }
}