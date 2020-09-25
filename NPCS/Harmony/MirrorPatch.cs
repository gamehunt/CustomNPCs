using HarmonyLib;
using Mirror;
using UnityEngine;
using System;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(NetworkBehaviour), nameof(NetworkBehaviour.SendTargetRPCInternal))]
    [HarmonyPriority(Priority.Last)]
    class MirrorPatch
    {
        private static bool Prefix(NetworkBehaviour __instance, NetworkConnection conn, Type invokeClass, string rpcName, NetworkWriter writer, int channelId)
        {
			if (!NetworkServer.active)
			{
				Debug.LogError("TargetRPC Function " + rpcName + " called on client.");
				return false;
			}
			if (conn == null)
			{
				conn = __instance.connectionToClient;
				if(conn == null)
                {
					return false;
                }
			}
			if (conn is ULocalConnectionToServer)
			{
				Debug.LogError("TargetRPC Function " + rpcName + " called on connection to server");
				return false;
			}
			if (!__instance.isServer)
			{
				Debug.LogWarning("TargetRpc " + rpcName + " called on un-spawned object: " + __instance.name);
				return false;
			}
			RpcMessage msg = new RpcMessage
			{
				netId = __instance.netId,
				componentIndex = __instance.ComponentIndex,
				functionHash = NetworkBehaviour.GetMethodHash(invokeClass, rpcName),
				payload = writer.ToArraySegment()
			};
			conn.Send<RpcMessage>(msg, channelId);
			return false;
		}
    }
}
