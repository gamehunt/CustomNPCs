using System;
using System.Collections.Generic;
using System.Text;

using Mirror;

namespace NPCS
{
    class Utils
    {
		public static void MakeCustomSyncVarWriter(NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customSyncVar, NetworkWriter owner, NetworkWriter observer)
		{
			ulong dirty = 0ul;
			ulong dirty_o = 0ul;
			NetworkBehaviour behaviour = null;
			for (int i = 0; i < behaviorOwner.NetworkBehaviours.Length; i++)
			{
				behaviour = behaviorOwner.NetworkBehaviours[i];
				if (behaviour.GetType() == targetType)
				{
					dirty |= 1UL << i;
					if (behaviour.syncMode == SyncMode.Observers) dirty_o |= 1UL << i;
				}
			}
			owner.WritePackedUInt64(dirty);
			observer.WritePackedUInt64(dirty & dirty_o);

			int position = owner.Position;
			owner.WriteInt32(0);
			int position2 = owner.Position;

			behaviour.SerializeObjectsDelta(owner);
			customSyncVar(owner);
			int position3 = owner.Position;
			owner.Position = position;
			owner.WriteInt32(position3 - position2);
			owner.Position = position3;

			if (dirty_o != 0ul)
			{
				ArraySegment<byte> arraySegment = owner.ToArraySegment();
				observer.WriteBytes(arraySegment.Array, position, owner.Position - position);
			}
		}
	}
}
