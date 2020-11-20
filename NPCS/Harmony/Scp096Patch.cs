using HarmonyLib;
using PlayableScps;
using UnityEngine;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.UpdateVision))]
    internal class Scp096Patch
    {
        private static bool Prefix(Scp096 __instance)
        {
			if (__instance._flash.Enabled)
			{
				return false;
			}
			Exiled.API.Features.Player scp096 = Exiled.API.Features.Player.Get(__instance.Hub);
			if(scp096.IsNPC() && !NPCS.Npc.Dictionary[__instance.Hub.gameObject].ProcessSCPLogic)
            {
				return false;
            }
			Vector3 vector = __instance.Hub.transform.TransformPoint(Scp096._headOffset);
			foreach (System.Collections.Generic.KeyValuePair<GameObject, global::ReferenceHub> keyValuePair in global::ReferenceHub.GetAllHubs())
			{
				global::ReferenceHub value = keyValuePair.Value;
				global::CharacterClassManager characterClassManager = value.characterClassManager;
				NPCS.Npc npc = NPCS.Npc.Dictionary.ContainsKey(keyValuePair.Key) ? NPCS.Npc.Dictionary[keyValuePair.Key] : null;
				if (characterClassManager.CurClass != global::RoleType.Spectator && !(value == __instance.Hub) && !characterClassManager.IsAnyScp() && Vector3.Dot((value.PlayerCameraReference.position - vector).normalized, __instance.Hub.PlayerCameraReference.forward) >= 0.1f && (npc == null || npc.ShouldTrigger096))
				{
					VisionInformation visionInformation = VisionInformation.GetVisionInformation(value, vector, -0.1f, 60f, true, true, __instance.Hub.localCurrentRoomEffects);
					if (visionInformation.IsLooking)
					{
						float delay = visionInformation.LookingAmount / 0.25f * (visionInformation.Distance * 0.1f);
						if (!__instance.Calming)
						{
							__instance.AddTarget(value.gameObject);
						}
						if (__instance.CanEnrage && value.gameObject != null)
						{
							__instance.PreWindup(delay);
						}
					}
				}
			}
			return false;
		}
    }
}