using HarmonyLib;
using UnityEngine;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.Update))]
    public class Scp079RecontaimentPatch
    {
        private static void Prefix(NineTailedFoxAnnouncer __instance)
        {
            if (global::NineTailedFoxAnnouncer.scpDeaths.Count <= 0)
            {
                return;
            }
            __instance.scpListTimer += Time.deltaTime;
            if (__instance.scpListTimer <= 1f)
            {
                return;
            }
            for (int i = 0; i < global::NineTailedFoxAnnouncer.scpDeaths.Count; i++)
            {
                string text = "";
                for (int j = 0; j < global::NineTailedFoxAnnouncer.scpDeaths[i].scpSubjects.Count; j++)
                {
                    string text2 = "";
                    string text3 = global::NineTailedFoxAnnouncer.scpDeaths[i].scpSubjects[j].fullName.Split(new char[]
                    {
                    '-'
                    })[1];
                    for (int k = 0; k < text3.Length; k++)
                    {
                        text2 = text2 + text3[k].ToString() + " ";
                    }
                    if (j == 0)
                    {
                        text = text + "SCP " + text2;
                    }
                    else
                    {
                        text = text + ". SCP " + text2;
                    }
                }
                global::NineTailedFoxAnnouncer.ScpDeath scpDeath = global::NineTailedFoxAnnouncer.scpDeaths[i];
                global::DamageTypes.DamageType damageType = scpDeath.hitInfo.GetDamageType();
                if (damageType == global::DamageTypes.Tesla)
                {
                    text += "SUCCESSFULLY TERMINATED BY AUTOMATIC SECURITY SYSTEM";
                }
                else if (damageType == global::DamageTypes.Nuke)
                {
                    text += "SUCCESSFULLY TERMINATED BY ALPHA WARHEAD";
                }
                else if (damageType == global::DamageTypes.Decont)
                {
                    text += "LOST IN DECONTAMINATION SEQUENCE";
                }
                else
                {
                    global::CharacterClassManager characterClassManager = null;
                    foreach (GameObject gameObject in global::PlayerManager.players)
                    {
                        int playerId = gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId;
                        scpDeath = global::NineTailedFoxAnnouncer.scpDeaths[i];
                        if (playerId == scpDeath.hitInfo.PlayerId)
                        {
                            characterClassManager = gameObject.GetComponent<global::CharacterClassManager>();
                        }
                    }
                    if (characterClassManager != null)
                    {
                        if (global::NineTailedFoxAnnouncer.scpDeaths[i].scpSubjects[0].roleId != global::RoleType.Scp106)
                        {
                            goto IL_207;
                        }
                        scpDeath = global::NineTailedFoxAnnouncer.scpDeaths[i];
                        if (scpDeath.hitInfo.GetDamageType() != global::DamageTypes.RagdollLess)
                        {
                            goto IL_207;
                        }
                        string text4 = "CONTAINEDSUCCESSFULLY";
                    IL_213:
                        string str = text4;
                        switch (characterClassManager.CurRole.team)
                        {
                            case global::Team.MTF:
                                {
                                    Respawning.NamingRules.UnitNamingRule unitNamingRule;
                                    string str2;
                                    if (!Respawning.NamingRules.UnitNamingRules.TryGetNamingRule(Respawning.SpawnableTeamType.NineTailedFox, out unitNamingRule))
                                    {
                                        str2 = "UNKNOWN";
                                    }
                                    else
                                    {
                                        str2 = unitNamingRule.GetCassieUnitName(characterClassManager.CurUnitName);
                                    }
                                    text = text + "CONTAINEDSUCCESSFULLY CONTAINMENTUNIT " + str2;
                                    goto IL_2BB;
                                }
                            case global::Team.CHI:
                                text = text + str + " BY CHAOSINSURGENCY";
                                goto IL_2BB;
                            case global::Team.RSC:
                                text = text + str + " BY SCIENCE PERSONNEL";
                                goto IL_2BB;
                            case global::Team.CDP:
                                text = text + str + " BY CLASSD PERSONNEL";
                                goto IL_2BB;
                            default:
                                text += "SUCCESSFULLY TERMINATED . CONTAINMENTUNIT UNKNOWN";
                                goto IL_2BB;
                        }
                    IL_207:
                        text4 = "TERMINATED";
                        goto IL_213;
                    }
                    text += "SUCCESSFULLY TERMINATED . TERMINATION CAUSE UNSPECIFIED";
                }
            IL_2BB:
                int num = 0;
                bool flag = false;
                foreach (GameObject gameObject2 in global::PlayerManager.players)
                {
                    global::CharacterClassManager component = gameObject2.GetComponent<global::CharacterClassManager>();
                    if (Npc.Dictionary.ContainsKey(gameObject2) && !Npc.Dictionary[gameObject2].AffectRoundSummary)
                    {
                        continue;
                    }
                    if (component.CurClass == global::RoleType.Scp079)
                    {
                        flag = true;
                    }
                    if (component.CurRole.team == global::Team.SCP)
                    {
                        num++;
                    }
                }
                if (num == 1 && flag && global::Generator079.mainGenerator.totalVoltage <= 4 && !global::Generator079.mainGenerator.forcedOvercharge)
                {
                    global::Generator079.mainGenerator.forcedOvercharge = true;
                    global::Recontainer079.BeginContainment(true);
                    text += " . ALLSECURED . SCP 0 7 9 RECONTAINMENT SEQUENCE COMMENCING . FORCEOVERCHARGE";
                }
                float num2 = (global::AlphaWarheadController.Host.timeToDetonation <= 0f) ? 3.5f : 1f;
                __instance.ServerOnlyAddGlitchyPhrase(text, UnityEngine.Random.Range(0.1f, 0.14f) * num2, UnityEngine.Random.Range(0.07f, 0.08f) * num2);
            }
            __instance.scpListTimer = 0f;
            global::NineTailedFoxAnnouncer.scpDeaths.Clear();
        }
    }
}