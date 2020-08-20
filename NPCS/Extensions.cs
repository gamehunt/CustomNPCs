using Mirror;
using System;

namespace NPCS
{
    public static class Extensions
    {
        public static void SendCustomSyncVar(this ReferenceHub player, NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customSyncVar)
        {
            /*
			Example:
			player.SendCustomSyncVar(player.networkIdentity, typeof(ServerRoles), (targetwriter) =>
			{
				targetwriter.WritePackedUInt64(2UL);
				targetwriter.WriteString("test");
			});
			 */
            NetworkWriter writer = NetworkWriterPool.GetWriter();
            NetworkWriter writer2 = NetworkWriterPool.GetWriter();
            Utils.MakeCustomSyncVarWriter(behaviorOwner, targetType, customSyncVar, writer, writer2);
            NetworkServer.SendToClientOfPlayer(player.queryProcessor.netIdentity, new UpdateVarsMessage() { netId = behaviorOwner.netId, payload = writer.ToArraySegment() });
            NetworkWriterPool.Recycle(writer);
            NetworkWriterPool.Recycle(writer2);
        }
    }
}