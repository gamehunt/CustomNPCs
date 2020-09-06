using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using HarmonyLib;
using UnityEngine;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(VersionCheck), nameof(VersionCheck.Start))]
    class VersionCheckFix
    {
        private static bool Prefix(VersionCheck __instance)
        {
            if(__instance.connectionToClient == null)
            {
                return false;
            }
            return true;
        }
    }
}
