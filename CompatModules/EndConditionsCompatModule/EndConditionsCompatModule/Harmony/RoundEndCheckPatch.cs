using Exiled.API.Features;
using Exiled.Events.EventArgs;
using HarmonyLib;
using NorthwoodLib.Pools;
using System.Collections.Generic;
using System.Linq;
using NPCS;
using EndConditions;
using EndConditions.Commands;

namespace EndConditionsCompatModule.Harmony
{
    [HarmonyPatch(typeof(EndConditions.EventHandlers), nameof(EndConditions.EventHandlers.OnCheckRoundEnd))]
    public class RoundEndCheckPatch
    {
        private static IEnumerable<string> GetRoles(bool ignore)
        {
            foreach (Player ply in Player.List)
            {
                if (ply.IsNPC())
                {
                    continue;
                }
                if (ply == null ||
                    string.IsNullOrEmpty(ply.UserId) ||
                    ply.Role == RoleType.Spectator ||
                    EndConditions.API.BlacklistedPlayers.Contains(ply))
                {
                    continue;
                }

                if (EndConditions.API.ModifiedRoles.TryGetValue(ply, out string modifiedRole))
                {
                    yield return modifiedRole.ToLower();
                    continue;
                }

                if (ply.SessionVariables.ContainsKey("IsScp035"))
                {
                    yield return "scp035";
                    continue;
                }

                if (ply.Role == RoleType.Tutorial && ignore)
                    continue;

                yield return ply.Role.ToString().ToLower();
            }
        }

        private static bool Prefix(EndConditions.EventHandlers __instance, EndingRoundEventArgs ev)
        {
            if (!__instance._config.AllowDefaultEndConditions)
            {
                ev.IsAllowed = false;
                ev.IsRoundEnded = false;
            }

            if (Warhead.IsDetonated && __instance._config.EndOnDetonation)
            {
                Log.Debug("Ending the round via warhead detonation.", __instance._config.AllowDebug);
                __instance.EndGame(ev, __instance._config.DetonationWinner);
                return false;
            }

            __instance.EscapeTracking["-classd"] = RoundSummary.escaped_ds == 0;
            __instance.EscapeTracking["+classd"] = RoundSummary.escaped_ds > 0;
            __instance.EscapeTracking["-science"] = RoundSummary.escaped_scientists == 0;
            __instance.EscapeTracking["+science"] = RoundSummary.escaped_scientists > 0;

            IEnumerable<string> roles = GetRoles(__instance._config.IgnoreTutorials);

            // Pull all the lists from the core dictionary and check em
            foreach (Condition condition in EndConditions.EventHandlers.Conditions.Where(condition => !roles.Except(condition.RoleConditions).Any()))
            {
                Log.Debug($"Using conditions from condition name: '{condition.Name}'", __instance._config.AllowDebug);

                // Check escape conditions
                List<string> failedConditions = ListPool<string>.Shared.Rent(condition.EscapeConditions.Where(cond => !__instance.EscapeTracking[cond]));
                if (failedConditions.Count > 0)
                {
                    Log.Debug($"Escape conditions failed at: {string.Join(", ", failedConditions)}", __instance._config.AllowDebug);
                    ListPool<string>.Shared.Return(failedConditions);
                    continue;
                }

                Log.Debug($"Escape checks passed: {string.Join(", ", condition.EscapeConditions)}", __instance._config.AllowDebug);
                ListPool<string>.Shared.Return(failedConditions);
                __instance.EndGame(ev, condition.LeadingTeam);
                return false;
            }
            return false;
        }
    }
}