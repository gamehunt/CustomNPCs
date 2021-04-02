using Exiled.API.Features;
using HarmonyLib;
using SerpentsHand;
using System.Collections.Generic;
using UnityEngine;
using NPCS;

namespace SHCompatModule.Harmony
{
    [HarmonyPatch(typeof(SerpentsHand.EventHandlers), nameof(SerpentsHand.EventHandlers.CountRoles))]
    public class SHCheckPatch
    {
        private static bool Prefix(SerpentsHand.EventHandlers __instance, ref int __result, Team team)
        {
			Player player = null;
			if (SerpentsHand.SerpentsHand.IsScp035)
			{
				player = __instance.TryGet035();
			}
			int num = 0;
			foreach (Player player2 in Player.List)
			{
                if (player2.IsNPC())
                {
					continue;
                }
				if (player2.Team == team && (player == null || player2.Id != player.Id))
				{
					num++;
				}
			}
			__result = num;
			return false;
		}
    }
}