using HarmonyLib;
using UnityEngine;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Update))]
    internal class PlayerStatsUpdateFix
    {
        private static bool Prefix(PlayerStats __instance)
        {
            if (!Npc.Dictionary.ContainsKey(__instance.gameObject))
            {
                return true;
            }
            if (!__instance._hpDirty)
            {
                return false;
            }
            __instance._hpDirty = false;
            foreach (GameObject gameObject in global::PlayerManager.players)
            {
                global::CharacterClassManager component = gameObject.GetComponent<global::CharacterClassManager>();
                if (component.CurClass == global::RoleType.Spectator && component.IsVerified && !Npc.Dictionary.ContainsKey(gameObject))
                {
                    __instance.TargetSyncHp(component.connectionToClient, __instance._health);
                }
            }
            return false;
        }
    }
}