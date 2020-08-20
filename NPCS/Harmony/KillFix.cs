using HarmonyLib;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetClassIDAdv))]
    [HarmonyPriority(Priority.First)]
    internal class KillFix
    {
        private static bool Prefix(CharacterClassManager __instance, global::RoleType id, bool lite, bool escape)
        {
            if ((__instance.isLocalPlayer && global::ServerStatic.IsDedicated) || !__instance.IsVerified)
            {
                return false;
            }
            if (NetworkServer.active)
            {
                __instance._hub.FriendlyFireHandler.Respawn.Reset();
                if (id == global::RoleType.Spectator)
                {
                    if (__instance._pms != null || __instance.SrvRoles == null || global::PermissionsHandler.IsPermitted(__instance.SrvRoles.Permissions, global::PlayerPermissions.AFKImmunity) || __instance.SrvRoles.OverwatchEnabled)
                    {
                        __instance._pms.IsAFK = false;
                    }
                    __instance._hub.FriendlyFireHandler.Life.Reset();
                }
            }
            if (!__instance.IsVerified && id != global::RoleType.Spectator)
            {
                return false;
            }
            if (id == global::RoleType.Tutorial && global::ServerStatic.IsDedicated && !GameCore.ConfigFile.ServerConfig.GetBool("allow_playing_as_tutorial", true))
            {
                return false;
            }
            if (__instance.SrvRoles.OverwatchEnabled && id != global::RoleType.Spectator)
            {
                if (__instance.CurClass == global::RoleType.Spectator)
                {
                    return false;
                }
                id = global::RoleType.Spectator;
            }
            __instance.DeathTime = ((id == global::RoleType.Spectator) ? DateTime.UtcNow.Ticks : 0L);
            __instance.NetworkCurClass = id;
            bool flag = id == global::RoleType.Spectator;
            if (NetworkServer.active)
            {
                if (flag)
                {
                    using (Dictionary<GameObject, global::ReferenceHub>.ValueCollection.Enumerator enumerator = global::ReferenceHub.GetAllHubs().Values.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            global::ReferenceHub referenceHub = enumerator.Current;
                            if (!referenceHub.isDedicatedServer && referenceHub.isReady)
                            {
                                global::PlayerStats playerStats = referenceHub.playerStats;
                                if (__instance.gameObject.GetComponent<NPCComponent>() == null)
                                {
                                    playerStats.TargetSyncHp(__instance.connectionToClient, playerStats.Health);
                                }
                            }
                        }
                        goto IL_1DA;
                    }
                }
                foreach (global::ReferenceHub referenceHub2 in global::ReferenceHub.GetAllHubs().Values)
                {
                    if (referenceHub2.characterClassManager != __instance && !referenceHub2.isDedicatedServer)
                    {
                        if (__instance.gameObject.GetComponent<NPCComponent>() == null)
                        {
                            referenceHub2.playerStats.TargetSyncHp(__instance.connectionToClient, -1f);
                        }
                    }
                }
            IL_1DA:
                __instance._hub.playerStats.MakeHpDirty();
                __instance._hub.playerStats.unsyncedArtificialHealth = 0f;
            }
            if (flag && !__instance.isLocalPlayer)
            {
                return false;
            }
            __instance.AliveTime = 0f;
            __instance.ApplyProperties(lite, escape);
            return false;
        }
    }
}