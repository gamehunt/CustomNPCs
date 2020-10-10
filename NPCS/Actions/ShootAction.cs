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
                if (!p.ReferenceHub.characterClassManager.IsAnyScp())
                {
                    npc.NPCPlayer.ReferenceHub.weaponManager.CallCmdShoot(p.GameObject, hitbox, npc.NPCPlayer.CameraTransform.forward, npc.NPCPlayer.Position, p.Position);
                    yield return Timing.WaitForSeconds(npc.NPCPlayer.ReferenceHub.weaponManager._fireCooldown);
                }
                else
                {
                    if (npc.NPCPlayer.Role.Is939())
                    {
                        npc.NPCPlayer.GameObject.GetComponent<Scp939PlayerScript>().CallCmdShoot(npc.CurrentAIPlayerTarget.GameObject);
                    }
                    else
                    {
                        switch (npc.NPCPlayer.Role)
                        {
                            case RoleType.Scp106:
                                npc.NPCPlayer.GameObject.GetComponent<Scp106PlayerScript>().CallCmdMovePlayer(npc.CurrentAIPlayerTarget.GameObject, ServerTime.time);
                                npc.Stop();
                                break;

                            case RoleType.Scp173:
                                npc.NPCPlayer.GameObject.GetComponent<Scp173PlayerScript>().CallCmdHurtPlayer(npc.CurrentAIPlayerTarget.GameObject);
                                break;

                            case RoleType.Scp049:
                                npc.CurrentAIPlayerTarget.Hurt(99999f, DamageTypes.Scp049, npc.NPCPlayer.Nickname);
                                break;

                            case RoleType.Scp0492:
                                npc.NPCPlayer.GameObject.GetComponent<Scp049_2PlayerScript>().CallCmdShootAnim();
                                npc.NPCPlayer.GameObject.GetComponent<Scp049_2PlayerScript>().CallCmdHurtPlayer(npc.CurrentAIPlayerTarget.GameObject);
                                break;
                        }
                    }
                    yield return Timing.WaitForSeconds(1f);
                }
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