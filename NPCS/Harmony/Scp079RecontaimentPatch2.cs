using HarmonyLib;
using UnityEngine;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.CheckForZombies))]
    internal class Scp079RecontaimentPatch2
    {
        private static void Prefix(GameObject zombie)
        {
            int num = 0;
            foreach (GameObject gameObject in global::PlayerManager.players)
            {
                if (Npc.Dictionary.ContainsKey(gameObject) && !Npc.Dictionary[gameObject].AffectRoundSummary)
                {
                    continue;
                }
                if (!(gameObject == zombie))
                {
                    global::ReferenceHub hub = global::ReferenceHub.GetHub(gameObject);
                    if (hub.characterClassManager.CurClass != global::RoleType.Scp079 && hub.characterClassManager.CurRole.team == global::Team.SCP)
                    {
                        num++;
                    }
                }
            }
            if (num <= 0 && global::Generator079.mainGenerator.totalVoltage < 4 && !global::Generator079.mainGenerator.forcedOvercharge)
            {
                global::Generator079.mainGenerator.forcedOvercharge = true;
                global::Recontainer079.BeginContainment(true);
                global::NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase("ALLSECURED . SCP 0 7 9 RECONTAINMENT SEQUENCE COMMENCING . FORCEOVERCHARGE", 0.1f, 0.07f);
            }
        }
    }
}