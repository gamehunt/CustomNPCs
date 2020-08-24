using Exiled.API.Features;
using MEC;
using NPCS.Talking;
using System.Collections.Generic;
using UnityEngine;

namespace NPCS.Actions
{
    internal class ShootAction : NodeAction
    {
        public override string Name => "ShootAction";

        private IEnumerator<float> ShootCoroutine(Npc npc, Player p, string hitbox, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                npc.NPCPlayer.ReferenceHub.weaponManager.CallCmdShoot(p.GameObject, hitbox, npc.NPCPlayer.CameraTransform.forward, npc.NPCPlayer.Position, p.Position);
                yield return Timing.WaitForSeconds(npc.NPCPlayer.ReferenceHub.weaponManager._fireCooldown);
            }
        }

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            Vector3 heading = (player.Position - npc.NPCPlayer.Position);
            Quaternion lookRot = Quaternion.LookRotation(heading.normalized);
            npc.NPCPlayer.Rotations = new Vector2(lookRot.eulerAngles.x, lookRot.eulerAngles.y);
            npc.AttachedCoroutines.Add(Timing.RunCoroutine(ShootCoroutine(npc, player, args["hitbox"], int.Parse(args["amount"]))));
        }
    }
}