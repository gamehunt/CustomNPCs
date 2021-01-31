using Exiled.API.Features;
using HarmonyLib;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ReversePlagueCompatModule.Harmony
{
    [HarmonyPatch(typeof(ReversePlague.EventHandlers), nameof(ReversePlague.EventHandlers.OnRoundStart))]
    internal class Scp049Healing
    {
        private static IEnumerator<float> BeginPlagueCoroutine(ReversePlague.EventHandlers __instance)
        {
            while (__instance.isRoundStarted)
            {
                foreach (ReferenceHub referenceHub in from x in Player.List
                                                      where !x.IsNPC() && x.Role == RoleType.Scp049
                                                      select x into p
                                                      select p.ReferenceHub)
                {
                    foreach (ReferenceHub referenceHub2 in Player.List.Where((Player x) => x.Role == RoleType.Scp0492 && !x.IsNPC()).Select((Player p) => p.ReferenceHub))
                    {
                        int num = (int)referenceHub.playerStats.Health + RPPlugin.plugin.Config.scp049HealAmount;
                        referenceHub.playerStats.Health = (float)((num > referenceHub.playerStats.maxHP) ? referenceHub.playerStats.maxHP : num);
                        foreach (ReferenceHub referenceHub3 in Player.List.Where(delegate (Player x)
                        {
                            if (x.Role == RoleType.Scp049)
                            {
                                return false;
                            }
                            if (!RPPlugin.plugin.Config.tutorialHeal)
                            {
                                return x.Team == Team.SCP;
                            }
                            return x.Team == Team.SCP || x.Team == Team.TUT;
                        }).Select((Player p) => p.ReferenceHub))
                        {
                            if (Vector3.Distance(referenceHub.transform.position, referenceHub3.transform.position) < RPPlugin.plugin.Config.range)
                            {
                                int num2 = (int)referenceHub3.playerStats.Health + RPPlugin.plugin.Config.scpHealAmount;
                                referenceHub3.playerStats.Health = (float)((num2 > referenceHub3.playerStats.maxHP) ? referenceHub3.playerStats.maxHP : num2);
                            }
                        }
                    }
                }
                yield return Timing.WaitForSeconds(RPPlugin.plugin.Config.interval);
            }
            yield break;
        }

        private static bool Prefix(ReversePlague.EventHandlers __instance)
        {
            __instance.isRoundStarted = true;
            Timing.RunCoroutine(BeginPlagueCoroutine(__instance));
            return false;
        }
    }
}