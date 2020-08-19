using System;
using System.Collections.Generic;
using System.Text;

using HarmonyLib;
using UnityEngine;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.ForcePosition),new Type[]{typeof(Vector3),typeof(string),typeof(bool)})]
    [HarmonyPriority(Priority.First)]
    class BringFix
    {
        private static bool Prefix(PlayerMovementSync __instance, Vector3 pos, string anticheatCode, bool reset)
        {
			if(__instance.gameObject.GetComponent<NPCComponent>() == null)
            {
				return true;
            }
			if (anticheatCode != null && global::PlayerMovementSync.AnticheatConsoleOutput)
			{
				global::ServerConsole.AddLog(string.Concat(new string[]
				{
				"[Anticheat Output] Player ",
				__instance._hub.nicknameSync.MyNick,
				" (",
				__instance._hub.characterClassManager.UserId,
				") has been teleported. Detection code: ",
				anticheatCode,
				"."
				}), ConsoleColor.Gray);
			}
			__instance.RealModelPosition = pos;
			__instance._lastSafePosition = pos;
			__instance._receivedPosition = pos;
			__instance._hub.falldamage._previousHeight = pos.y;
			__instance._groundedY = pos.y;
			__instance._flyTime = 0f;
			if (anticheatCode == null || reset)
			{
				__instance._resetS = 0f;
				__instance._resetL = 0f;
				__instance._violationsS = 0;
				__instance._violationsL = 0;
			}
			else if (!__instance._suppressViolations)
			{
				__instance._violationsS += 1;
				__instance._violationsL += 1;
			}
			__instance._positionForced = true;
			__instance._suppressViolations = true;
			__instance._forcedPosTime = 0f;
			if (__instance._corroding.Enabled && pos.y > -1900f)
			{
				__instance._corroding.ServerDisable();
			}
			__instance.AddSafeTime(0.8f);
			return false;
		}
    }
}
