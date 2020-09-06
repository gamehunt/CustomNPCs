using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Mirror;
using MEC;
namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.Init))]
    class VerificationFix
    {
        private static bool Prefix(CharacterClassManager __instance)
        {
			if(!Npc.Dictionary.ContainsKey(__instance.gameObject))
            {
                return true;
            }
            __instance.IsVerified = true;
            return false;
		}
    }
}
