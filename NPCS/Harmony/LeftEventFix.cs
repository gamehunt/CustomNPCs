using Exiled.API.Features;
using Exiled.Events.EventArgs;
using HarmonyLib;
using System;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.OnDestroy))]
    internal class LeftEventFix
    {
        private static void Prefix(ReferenceHub __instance)
        {
            try
            {
                Player player = Player.Get(__instance.gameObject);

                if (player == null || player.IsHost)
                    return;

                var ev = new LeftEventArgs(player);

                if (!Npc.Dictionary.ContainsKey(__instance.gameObject))
                {
                    Log.SendRaw($"Player {ev.Player.Nickname} ({ev.Player.UserId}) ({player?.Id}) disconnected", ConsoleColor.Green);
                }
                else
                {
                    Log.SendRaw($"NPC {ev.Player.Nickname} ({player?.Id}) deconstructed", ConsoleColor.Green);
                }

                Exiled.Events.Handlers.Player.OnLeft(ev);

                Player.IdsCache.Remove(player.Id);
                if (!player.IsNPC())
                {
                    Player.UserIdsCache.Remove(player.UserId);
                }
                Player.Dictionary.Remove(player.GameObject);
            }
            catch (Exception exception)
            {
                Log.Error($"Exiled.Events.Patches.Events.Player.Left (Repatched by CustomNPCs!!!): {exception}\n{exception.StackTrace}");
            }
        }
    }
}