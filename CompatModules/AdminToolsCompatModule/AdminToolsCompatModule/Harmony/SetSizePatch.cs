using Exiled.API.Features;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using NPCS;
using System;
using Mirror;
using Exiled.API.Extensions;

namespace AdminToolsCompatModule.Harmony
{
    [HarmonyPatch(typeof(AdminTools.EventHandlers), nameof(AdminTools.EventHandlers.SetPlayerScale), new Type[] { typeof(GameObject), typeof(float), typeof(float), typeof(float) })]
    public class SetSizePatch
    {
        private static bool Prefix(AdminTools.EventHandlers __instance, GameObject target, float x, float y, float z)
        {
			try
			{
				NetworkIdentity component = target.GetComponent<NetworkIdentity>();
				target.transform.localScale = new Vector3(1f * x, 1f * y, 1f * z);
				ObjectDestroyMessage msg = default(ObjectDestroyMessage);
				msg.netId = component.netId;
				foreach (GameObject gameObject in PlayerManager.players)
				{
					NetworkConnection connectionToClient = gameObject.GetComponent<NetworkIdentity>().connectionToClient;
					if(connectionToClient == null)
                    {
						continue;
                    }
					if (gameObject != target)
					{
						connectionToClient.Send<ObjectDestroyMessage>(msg, 0);
					}
					object[] param = new object[]
					{
						component,
						connectionToClient
					};
					typeof(NetworkServer).InvokeStaticMethod("SendSpawnMessage", param);
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