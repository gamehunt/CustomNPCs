using Exiled.Events.EventArgs;
using HarmonyLib;
using NPCS;

namespace TeslaGateControlCompatModule.Harmony
{
    [HarmonyPatch(typeof(TeslaGateControl.eventHandlers), nameof(TeslaGateControl.eventHandlers.PlayerTesla))]
    internal class TeslaEventPatch
    {
        private static bool Prefix(TeslaGateControl.eventHandlers __instance, TriggeringTeslaEventArgs ev)
        {
            if(ev.Player.IsNPC())
            {
                ev.IsTriggerable = false;
                return false;
            }
            return true;
        }
    }
}