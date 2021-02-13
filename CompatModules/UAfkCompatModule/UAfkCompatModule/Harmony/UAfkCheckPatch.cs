using Exiled.API.Features;
using HarmonyLib;
using UltimateAFK;
using System.Collections.Generic;
using UnityEngine;
using NPCS;

namespace UAfkCompatModule.Harmony
{
    [HarmonyPatch(typeof(UltimateAFK.AFKComponent), nameof(UltimateAFK.AFKComponent.Update))]
    public class UAfkCheckPatch
    {
        private static bool Prefix(AFKComponent __instance)
        {
            if(!__instance.disabled && __instance.ply != null && __instance.ply.IsNPC())
            {
                __instance.disabled = true;
            }
            return true;
        }
    }
}