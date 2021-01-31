using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NPCS.AI
{
    //Runs while target is alive, resets nav
    internal class AIAttackTarget : AITarget
    {
        public override string Name => "AIAttackTarget";

        public override string[] RequiredArguments => new string[] { "accuracy", "hitboxes", "firerate", "damage", "use_ammo" };

        public override bool Check(Npc npc)
        {
            return npc.CurrentAIPlayerTarget != null && Player.Dictionary.ContainsKey(npc.CurrentAIPlayerTarget.GameObject) && npc.CurrentAIPlayerTarget.IsAlive && !Physics.Linecast(npc.NPCPlayer.Position, npc.CurrentAIPlayerTarget.Position, npc.NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces);
        }

        private int accuracy;
        private readonly Dictionary<HitBoxType, int> hitboxes = new Dictionary<HitBoxType, int>();
        private float firerate;
        private int damage;
        private bool use_ammo;

        public override void Construct()
        {
            accuracy = int.Parse(Arguments["accuracy"]);
            foreach (string val in Arguments["hitboxes"].Split(','))
            {
                string[] splitted = val.Trim().Split(':');
                hitboxes.Add((HitBoxType)Enum.Parse(typeof(HitBoxType), splitted[0]), int.Parse(splitted[1]));
            }
            firerate = float.Parse(Arguments["firerate"].Replace('.', ','));
            damage = int.Parse(Arguments["damage"]);
            use_ammo = bool.Parse(Arguments["use_ammo"]);
        }

        private IEnumerator<float> ReviveCoroutine(Npc npc, Player target)
        {
            yield return Timing.WaitForSeconds(PlayableScps.Scp049.TimeToRevive);
            if (npc.NPCPlayer.IsAlive && target.IsDead)
            {
                target.Role = RoleType.Scp0492;
                yield return Timing.WaitForSeconds(0.3f);
                target.Position = npc.NPCPlayer.Position;
            }
        }

        public override float Process(Npc npc)
        {
            if (!npc.NPCPlayer.ReferenceHub.characterClassManager.IsAnyScp())
            {
                if (npc.AvailableWeapons.Count > 0)
                {
                    if (!npc.ItemHeld.IsWeapon(false))
                    {
                        npc.ItemHeld = npc.AvailableWeapons.Keys.ElementAt(0);
                        return 0.5f;
                    }

                    npc.Stop();
                    Vector3 heading = (npc.CurrentAIPlayerTarget.Position - npc.NPCPlayer.Position);
                    Quaternion lookRot = Quaternion.LookRotation(heading.normalized);
                    npc.NPCPlayer.Rotations = new Vector2(lookRot.eulerAngles.x, lookRot.eulerAngles.y);
                    bool miss = Plugin.Random.Next(0, 100) >= accuracy;
                    int hitbox_value = Plugin.Random.Next(0, 100);
                    HitBoxType hitbox = HitBoxType.NULL;
                    int min = int.MaxValue;
                    foreach (HitBoxType box in hitboxes.Keys)
                    {
                        if (hitbox_value < hitboxes[box] && hitboxes[box] <= min)
                        {
                            min = hitboxes[box];
                            hitbox = box;
                        }
                    }

                    npc.NPCPlayer.ReferenceHub.weaponManager.CallCmdShoot(miss ? npc.gameObject : npc.CurrentAIPlayerTarget.GameObject, hitbox, npc.NPCPlayer.CameraTransform.forward, npc.NPCPlayer.Position, npc.CurrentAIPlayerTarget.Position);

                    bool end = !npc.CurrentAIPlayerTarget.IsAlive;

                    if (use_ammo)
                    {
                        npc.AvailableWeapons[npc.ItemHeld]--;
                        if (npc.AvailableWeapons[npc.ItemHeld] <= 0)
                        {
                            npc.NPCPlayer.ReferenceHub.weaponManager.RpcReload(npc.NPCPlayer.ReferenceHub.weaponManager.curWeapon);
                            npc.AvailableWeapons[npc.ItemHeld] = (int)npc.NPCPlayer.ReferenceHub.weaponManager.weapons[npc.NPCPlayer.ReferenceHub.weaponManager.curWeapon].maxAmmo;
                            if (end)
                            {
                                npc.FireEvent(new Events.NPCTargetKilledEvent(npc, npc.CurrentAIPlayerTarget));
                                IsFinished = true;
                            }
                            return npc.NPCPlayer.ReferenceHub.weaponManager.weapons[npc.NPCPlayer.ReferenceHub.weaponManager.curWeapon].reloadingTime;
                        }
                    }

                    if (end)
                    {
                        npc.FireEvent(new Events.NPCTargetKilledEvent(npc, npc.CurrentAIPlayerTarget));
                        IsFinished = true;
                        return 0f;
                    }
                }
                else
                {
                    IsFinished = true;
                    return 0f;
                }
                return firerate * Plugin.Instance.Config.NpcFireCooldownMultiplier * npc.NPCPlayer.ReferenceHub.weaponManager._fireCooldown;
            }
            else
            {
                float cd = 0f;
                npc.OnTargetLostBehaviour = Npc.TargetLostBehaviour.STOP;
                npc.Follow(npc.CurrentAIPlayerTarget);
                if (Vector3.Distance(npc.CurrentAIPlayerTarget.Position, npc.NPCPlayer.Position) <= 1.5f)
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
                                cd = 2f;
                                break;

                            case RoleType.Scp173:
                                npc.NPCPlayer.GameObject.GetComponent<Scp173PlayerScript>().CallCmdHurtPlayer(npc.CurrentAIPlayerTarget.GameObject);
                                break;

                            case RoleType.Scp049:
                                npc.CurrentAIPlayerTarget.Hurt(99999f, DamageTypes.Scp049, npc.NPCPlayer.Nickname);
                                cd = PlayableScps.Scp049.KillCooldown;
                                break;

                            case RoleType.Scp0492:
                                npc.NPCPlayer.GameObject.GetComponent<Scp049_2PlayerScript>().CallCmdShootAnim();
                                npc.NPCPlayer.GameObject.GetComponent<Scp049_2PlayerScript>().CallCmdHurtPlayer(npc.CurrentAIPlayerTarget.GameObject);
                                cd = 1f;
                                break;

                            case RoleType.Scp096:
                                npc.CurrentAIPlayerTarget.Hurt(99999f, DamageTypes.Scp096, npc.NPCPlayer.Nickname, npc.NPCPlayer.Id);
                                break;
                        }
                    }
                    if (!npc.CurrentAIPlayerTarget.IsAlive)
                    {
                        npc.AttachedCoroutines.Add(MEC.Timing.CallDelayed(0.1f, () =>
                         {
                             npc.FireEvent(new Events.NPCTargetKilledEvent(npc, npc.CurrentAIPlayerTarget));
                         }));

                        npc.Stop();

                        Player target = npc.CurrentAIPlayerTarget;

                        npc.CurrentAIPlayerTarget = null;

                        if (npc.ProcessSCPLogic && npc.NPCPlayer.Role == RoleType.Scp049)
                        {
                            npc.AttachedCoroutines.Add(Timing.RunCoroutine(ReviveCoroutine(npc, target)));
                            IsFinished = true;
                            return PlayableScps.Scp049.TimeToRevive + 0.5f;
                        }
                    }
                }
                return cd;
            }
        }

        protected override AITarget CreateInstance()
        {
            return new AIAttackTarget();
        }
    }
}