using Exiled.API.Features;
using System.Linq;
using UnityEngine;

namespace NPCS.AI
{
    class AIShootTarget : AITarget
    {
        public override string Name => "AIShootTarget";

        public override bool Check(Npc npc)
        {
            return npc.CurrentAIPlayerTarget != null && Player.List.Contains(npc.CurrentAIPlayerTarget) && npc.CurrentAIPlayerTarget.IsAlive && !Physics.Linecast(npc.NPCPlayer.Position, npc.CurrentAIPlayerTarget.Position, npc.NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces);
        }

        public override float Process(Npc npc)
        {
            Vector3 heading = (npc.CurrentAIPlayerTarget.Position - npc.NPCPlayer.Position);
            Quaternion lookRot = Quaternion.LookRotation(heading.normalized);
            npc.NPCPlayer.Rotations = new Vector2(lookRot.eulerAngles.x, lookRot.eulerAngles.y);
            npc.NPCPlayer.ReferenceHub.weaponManager.CallCmdShoot(npc.CurrentAIPlayerTarget.GameObject, "HEAD", npc.NPCPlayer.CameraTransform.forward, npc.NPCPlayer.Position, npc.CurrentAIPlayerTarget.Position);
            return npc.NPCPlayer.ReferenceHub.weaponManager._fireCooldown;
        }

        protected override AITarget CreateInstance()
        {
            return new AIShootTarget();
        }
    }
}
