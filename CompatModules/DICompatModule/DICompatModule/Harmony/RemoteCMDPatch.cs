using Exiled.API.Features;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using NPCS;

namespace DICompatModule.Harmony
{
    [HarmonyPatch(typeof(DiscordIntegration.API.Commands.RemoteCommand), MethodType.Constructor)]
    public class RemoteCMDPatch
    {
        private static void Postfix(DiscordIntegration.API.Commands.RemoteCommand __instance , string action, object parameters)
        {
            if (action == "updateActivity")
            {
                __instance.Parameters[0] = $"{Player.Dictionary.Count - Npc.Dictionary.Count}/{DiscordIntegration.DiscordIntegration.Instance.Slots}";
            }
        }
    }
}