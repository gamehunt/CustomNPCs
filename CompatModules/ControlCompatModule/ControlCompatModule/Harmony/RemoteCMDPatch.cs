using Exiled.API.Features;
using HarmonyLib;
using System.Linq;
using NPCS;

namespace ControlCompatModule.Harmony
{
    [HarmonyPatch(typeof(Control.Plugin), nameof(Control.Plugin.GetQuickInfo))]
    public class RemoteCMDPatch
    {
        private static bool Prefix(Control.Plugin __instance, ref string __result)
        {
            __result = $"2 {Player.List.Count(p => !p.IsNPC())}/{Player.List.Count(x => !x.IsNPC() && __instance.Config.staffGroups.Contains(x.GroupName))}/{Player.List.Count(x => !x.IsNPC() && __instance.Config.adminGroups.Contains(x.GroupName))}";
            return false;
        }
    }
}