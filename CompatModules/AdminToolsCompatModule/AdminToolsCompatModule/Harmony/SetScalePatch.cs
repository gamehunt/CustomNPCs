using Exiled.API.Features;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using NPCS;
using System;
using Mirror;
using Exiled.API.Extensions;

namespace UAfkCompatModule.Harmony
{
    [HarmonyPatch(typeof(AdminTools.EventHandlers), nameof(AdminTools.EventHandlers.SetPlayerScale), new Type[] { typeof(GameObject), typeof(float), })]
    public class SetScalePatch
    {
        private static bool Prefix(AdminTools.EventHandlers __instance, GameObject target, float scale)
        {
			try
			{
				NetworkIdentity component = target.GetComponent<NetworkIdentity>();
				target.transform.localScale = Vector3.one * scale;
				ObjectDestroyMessage msg = default(ObjectDestroyMessage);
				msg.netId = component.netId;
				foreach (GameObject gameObject in PlayerManager.players)
				{
					if (!(gameObject == target))
					{
						NetworkConnection connectionToClient = gameObject.GetComponent<NetworkIdentity>().connectionToClient;
						if(connectionToClient == null)
                        {
							continue;
                        }
						connectionToClient.Send<ObjectDestroyMessage>(msg, 0);
						object[] param = new object[]
						{
							component,
							connectionToClient
						};
						typeof(NetworkServer).InvokeStaticMethod("SendSpawnMessage", param);
					}
				}
			}
			catch (Exception arg)
			{
				Log.Info(string.Format("Set Scale error: {0}", arg));
			}
			return false;
        }
    }
}