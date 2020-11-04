using Exiled.API.Extensions;
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

        private IEnumerator<float> ShootCoroutine(Npc npc, Player p, HitBoxType hitbox, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                if (!npc.NPCPlayer.ReferenceHub.characterClassManager.IsAnyScp())
                {
                    if (npc.AvailableWeapons.Length > 0)
                    {
                        if (!npc.ItemHeld.IsWeapon(false))
                        {
                            npc.ItemHeld = npc.AvailableWeapons[0];
                        }
                        npc.Stop();
                        Vector3 heading = (p.Position - npc.NPCPlayer.Position);
                        Quaternion lookRot = Quaternion.LookRotation(heading.normalized);
                        npc.NPCPlayer.Rotations = new Vector2(lookRot.eulerAngles.x, lookRot.eulerAngles.y);
                        npc.NPCPlayer.ReferenceHub.weaponManager.CallCmdShoot(p.GameObject, hitbox, npc.NPCPlayer.CameraTransform.forward, npc.NPCPlayer.Position, p.Position);
                    }
                    yield return Timing.WaitForSeconds(Plugin.Instance.Config.NpcFireCooldownMultiplier * npc.NPCPlayer.ReferenceHub.weaponManager._fireCooldown);
                }
                else
                {
                    float cd = 0f;
                    npc.Follow(p);
                    if (Vector3.Distance(p.Position, npc.NPCPlayer.Position) <= 1.5f)
                    {
                        if (npc.NPCPlayer.Role.Is939())
                        {
                            npc.NPCPlayer.GameObject.GetComponent<Scp939PlayerScript>().CallCmdShoot(p.GameObject);
                        }
                        else
                        {
                            switch (npc.NPCPlayer.Role)
                            {
                                case RoleType.Scp106:
                                    npc.NPCPlayer.GameObject.GetComponent<Scp106PlayerScript>().CallCmdMovePlayer(p.GameObject, ServerTime.time);
                                    cd = 2f;
                                    break;

                                case RoleType.Scp173:
                                    npc.NPCPlayer.GameObject.GetComponent<Scp173PlayerScript>().CallCmdHurtPlayer(p.GameObject);
                                    break;

                                case RoleType.Scp049:
                                    p.Hurt(99999f, DamageTypes.Scp049, npc.NPCPlayer.Nickname);
                                    cd = PlayableScps.Scp049.KillCooldown;
                                    break;

                                case RoleType.Scp0492:
                                    npc.NPCPlayer.GameObject.GetComponent<Scp049_2PlayerScript>().CallCmdShootAnim();
                                    npc.NPCPlayer.GameObject.GetComponent<Scp049_2PlayerScript>().CallCmdHurtPlayer(p.GameObject);
                                    cd = 1f;
                                    break;
                            }
                        }
                        npc.Stop();
                    }
                    yield return Timing.WaitForSeconds(cd);
                }
            }
        }

        public override void Process(Npc npc, Player player, Dictionary<string, string> args)
        {
            Vector3 heading = (player.Position - npc.NPCPlayer.Position);
            Quaternion lookRot = Quaternion.LookRotation(heading.normalized);
            npc.NPCPlayer.Rotations = new Vector2(lookRot.eulerAngles.x, lookRot.eulerAngles.y);
            npc.AttachedCoroutines.Add(Timing.RunCoroutine(ShootCoroutine(npc, player, (HitBoxType)System.Enum.Parse(typeof(HitBoxType),args["hitbox"]), int.Parse(args["amount"]))));
        }
    }
}