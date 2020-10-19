using Exiled.API.Extensions;
using Exiled.API.Features;
using System.Collections.Generic;
using UnityEngine;

namespace NPCS.AI
{
    //Runs while target is alive, resets nav
    internal class AIShootTarget : AITarget
    {
        public override string Name => "AIShootTarget";

        public override string[] RequiredArguments => new string[] { "accuracy", "hitboxes", "firerate", "damage" };

        public override bool Check(Npc npc)
        {
            return npc.CurrentAIPlayerTarget != null && Player.Dictionary.ContainsKey(npc.CurrentAIPlayerTarget.GameObject) && npc.CurrentAIPlayerTarget.IsAlive && !Physics.Linecast(npc.NPCPlayer.Position, npc.CurrentAIPlayerTarget.Position, npc.NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces);
        }

        private int accuracy;
        private Dictionary<string, int> hitboxes;
        private float firerate;
        private int damage;

        public override void Construct()
        {
            accuracy = int.Parse(Arguments["accuracy"]);
            foreach (string val in Arguments["hitboxes"].Split(','))
            {
                string[] splitted = val.Trim().Split(':');
                hitboxes.Add(splitted[0], int.Parse(splitted[1]));
            }
            firerate = float.Parse(Arguments["firerate"].Replace('.', ','));
            damage = int.Parse(Arguments["damage"]);
        }

        public override float Process(Npc npc)
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
                    Vector3 heading = (npc.CurrentAIPlayerTarget.Position - npc.NPCPlayer.Position);
                    Quaternion lookRot = Quaternion.LookRotation(heading.normalized);
                    npc.NPCPlayer.Rotations = new Vector2(lookRot.eulerAngles.x, lookRot.eulerAngles.y);
                    bool miss = Plugin.Random.Next(0, 100) < accuracy;
                    int hitbox_value = Plugin.Random.Next(0, 100);
                    string hitbox = "BODY";
                    int min = int.MaxValue;
                    foreach (string box in hitboxes.Keys)
                    {
                        if (hitbox_value < hitboxes[box] && hitboxes[box] <= min)
                        {
                            min = hitboxes[box];
                            hitbox = box;
                        }
                    }
                    npc.NPCPlayer.ReferenceHub.weaponManager.CallCmdShoot(miss ? npc.gameObject : npc.CurrentAIPlayerTarget.GameObject, damage > 0 ? "_:" + hitbox + ":" + damage.ToString() : hitbox, npc.NPCPlayer.CameraTransform.forward, npc.NPCPlayer.Position, npc.CurrentAIPlayerTarget.Position);
                    if (!npc.CurrentAIPlayerTarget.IsAlive)
                    {
                        npc.FireEvent(new Events.NPCTargetKilledEvent(npc, npc.CurrentAIPlayerTarget));
                    }
                }
                return firerate * Plugin.Instance.Config.NpcFireCooldownMultiplier * npc.NPCPlayer.ReferenceHub.weaponManager._fireCooldown;
            }
            else
            {
                float cd = 0f;
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
                        }
                    }
                    if (!npc.CurrentAIPlayerTarget.IsAlive)
                    {
                        npc.FireEvent(new Events.NPCTargetKilledEvent(npc, npc.CurrentAIPlayerTarget));
                    }
                    npc.Stop();
                }
                return cd;
            }
        }

        protected override AITarget CreateInstance()
        {
            return new AIShootTarget();
        }
    }
}