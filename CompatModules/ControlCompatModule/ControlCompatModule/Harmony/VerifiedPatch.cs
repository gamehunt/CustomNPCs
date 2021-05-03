using Exiled.API.Features;
using HarmonyLib;
using System.Linq;
using NPCS;
using Exiled.Events.EventArgs;

namespace ControlCompatModule.Harmony
{
    [HarmonyPatch(typeof(Control.Plugin), nameof(Control.Plugin.OnVerified))]
    public class VerifiedPatch
    {
        private static bool Prefix(Control.Plugin __instance, VerifiedEventArgs ev)
        {
            if (__instance.maxPlayers < Player.List.Count(x => !x.IsNPC()))
            {
                __instance.maxPlayers = Player.List.Count(x => !x.IsNPC());
                Control.Plugin.ws?.Send($"4 {__instance.maxPlayers}");
            }
            Control.Plugin.ws?.Send($"3 {ev.Player.UserId}");
            Control.Plugin.ws?.Send(__instance.GetQuickInfo());

            if (__instance.Config.staffGroups.Contains(ev.Player.GroupName))
            {
                Control.Plugin.ws?.Send($"11 0 {ev.Player.UserId} {ev.Player.Nickname}");
                __instance._staffOnline.Add(ev.Player.UserId);
            }

            __instance.CreateMessage("Verified", "(someone)", ev.Player.Nickname);
            return false;
        }
    }
}
