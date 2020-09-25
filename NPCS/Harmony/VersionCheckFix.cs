using HarmonyLib;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(VersionCheck), nameof(VersionCheck.Start))]
    internal class VersionCheckFix
    {
        private static bool Prefix(VersionCheck __instance)
        {
            return __instance.connectionToClient != null;
        }
    }
}