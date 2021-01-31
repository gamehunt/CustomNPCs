using Exiled.API.Features;
using HarmonyLib;
using System;
using UnityEngine;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(Player), nameof(Player.Scale), MethodType.Setter)]
    internal class SetScaleFix
    {
        private static bool Prefix(Player __instance, Vector3 value)
        {
            try
            {
                __instance.ReferenceHub.transform.localScale = value;

                foreach (Player target in Player.List)
                {
                    if (!target.IsNPC())
                    {
                        Server.SendSpawnMessage?.Invoke(null, new object[] { __instance.ReferenceHub.characterClassManager.netIdentity, target.Connection });
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error($"SetScale error: {exception}");
            }
            return false;
        }
    }
}