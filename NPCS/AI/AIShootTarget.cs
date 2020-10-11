using Exiled.API.Features;
using UnityEngine;

namespace NPCS.AI
{
    internal class AIShootTarget : AITarget
    {
        public override string Name => "AIShootTarget";

        public override bool Check(Npc npc)
        {
            return npc.CurrentAIPlayerTarget != null && Player.Dictionary.ContainsKey(npc.CurrentAIPlayerTarget.GameObject) && npc.CurrentAIPlayerTarget.IsAlive && Vector3.Distance(npc.NPCPlayer.Position,npc.CurrentAIPlayerTarget.Position) < 100f && !Physics.Linecast(npc.NPCPlayer.Position, npc.CurrentAIPlayerTarget.Position, npc.NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces);
        }

        public override float Process(Npc npc)
        {
            if (!npc.NPCPlayer.ReferenceHub.characterClassManager.IsAnyScp())
            {
                npc.Stop();
                Vector3 heading = (npc.CurrentAIPlayerTarget.Position - npc.NPCPlayer.Position);
                Quaternion lookRot = Quaternion.LookRotation(heading.normalized);
                npc.NPCPlayer.Rotations = new Vector2(lookRot.eulerAngles.x, lookRot.eulerAngles.y);
                npc.NPCPlayer.ReferenceHub.weaponManager.CallCmdShoot(npc.CurrentAIPlayerTarget.GameObject, "HEAD", npc.NPCPlayer.CameraTransform.forward, npc.NPCPlayer.Position, npc.CurrentAIPlayerTarget.Position);
            }
            else
            {
                npc.DisableFollowAutoTeleport = true;
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
                }
            }
            return npc.NPCPlayer.ReferenceHub.weaponManager._fireCooldown;
        }

        protected override AITarget CreateInstance()
        {
            return new AIShootTarget();
        }
    }
}