using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

namespace NPCS.Utils
{
    public class SerializableVector2
    {
        public SerializableVector2()
        {
        }

        public SerializableVector2(Vector2 vec)
        {
            x = vec.x;
            y = vec.y;
        }

        public float x { get; set; }
        public float y { get; set; }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
    }

    public class NPCAIHelper
    {
        public NPCAIHelper(Npc parent)
        {
            npc = parent;
        }

        private Npc npc;

        public List<Player> GetNearestPlayers(float range, bool include_npcs = false)
        {
            return Player.List.Where(pp => pp != npc.NPCPlayer && (!pp.IsNPC() || include_npcs) && Vector3.Distance(pp.Position, npc.NPCPlayer.Position) <= range).ToList();
        }

        public List<Player> GetNearestPlayers(float range, RoleType role, bool include_npcs = false)
        {
            return Player.Get(role).Where(pp => pp != npc.NPCPlayer && (!pp.IsNPC() || include_npcs) && Vector3.Distance(pp.Position, npc.NPCPlayer.Position) <= range).ToList();
        }

        public List<Player> GetNearestPlayers(float range, RoleType[] roles, bool IsBlacklist = false, bool include_npcs = false)
        {
            if (!IsBlacklist)
            {
                return Player.List.Where(pp => pp != npc.NPCPlayer && (!pp.IsNPC() || include_npcs) && Vector3.Distance(pp.Position, npc.NPCPlayer.Position) <= range && roles.Contains(pp.Role)).ToList();
            }
            else
            {
                return Player.List.Where(pp => pp != npc.NPCPlayer && (!pp.IsNPC() || include_npcs) && Vector3.Distance(pp.Position, npc.NPCPlayer.Position) <= range && !roles.Contains(pp.Role)).ToList();
            }
        }
    }

    public class NPCAIController
    {
        public NPCAIController(Npc parent)
        {
            Npc = parent;
        }

        public Npc Npc { get; }

        public Player CurrentPlayerTarget
        {
            get => Npc.CurrentAIPlayerTarget;
            set
            {
                Npc.CurrentAIPlayerTarget = value;
            }
        }

        public void Stop()
        {
            Npc.Stop();
        }

        public void Follow(Player target, Npc.TargetLostBehaviour behav = Npc.TargetLostBehaviour.SEARCH)
        {
            Npc.OnTargetLostBehaviour = behav;
            Npc.Follow(target);
        }

        public void GoToRoom(string room_name = "", bool safe = true)
        {
            if (string.IsNullOrEmpty(room_name))
            {
                List<Room> valid_rooms = Map.Rooms.Where(rm => rm.Zone != Exiled.API.Enums.ZoneType.LightContainment || (safe ? Round.ElapsedTime.Minutes < 10 : !Map.IsLCZDecontaminated)).ToList();
                Room r = valid_rooms[Plugin.Random.Next(0, valid_rooms.Count)];
                Log.Debug($"[AI] Room selected: {r.Name}", Plugin.Instance.Config.VerboseOutput);
                Npc.Stop();
                if (Npc.GotoRoom(r))
                {
                    Npc.CurrentAIRoomTarget = r;
                }
            }
            else
            {
                Room room = Map.Rooms.Where(rm => rm.Name.Equals(room_name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                Npc.Stop();
                if (Npc.GotoRoom(room))
                {
                    Npc.CurrentAIRoomTarget = room;
                }
            }
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

        public float Attack(Player target, int accuracy = 100, Dictionary<HitBoxType, int> hitboxes = null, bool use_ammo = false, float firerate_mul = 1f)
        {
            if (!Npc.NPCPlayer.ReferenceHub.characterClassManager.IsAnyScp())
            {
                if (Npc.AvailableWeapons.Count > 0)
                {
                    if (!Npc.ItemHeld.IsWeapon(false))
                    {
                        Npc.ItemHeld = Npc.AvailableWeapons.Keys.ElementAt(0);
                        return 0.5f;
                    }

                    Npc.Stop();
                    Vector3 heading = (Npc.CurrentAIPlayerTarget.Position - Npc.NPCPlayer.Position);
                    Quaternion lookRot = Quaternion.LookRotation(heading.normalized);
                    Npc.NPCPlayer.Rotations = new Vector2(lookRot.eulerAngles.x, lookRot.eulerAngles.y);
                    bool miss = Plugin.Random.Next(0, 100) >= accuracy;
                    int hitbox_value = Plugin.Random.Next(0, 100);
                    HitBoxType hitbox = HitBoxType.NULL;
                    int min = int.MaxValue;

                    if (hitboxes != null)
                    {
                        foreach (HitBoxType box in hitboxes.Keys)
                        {
                            if (hitbox_value < hitboxes[box] && hitboxes[box] <= min)
                            {
                                min = hitboxes[box];
                                hitbox = box;
                            }
                        }
                    }

                    Npc.NPCPlayer.ReferenceHub.weaponManager.CallCmdShoot(miss ? Npc.gameObject : Npc.CurrentAIPlayerTarget.GameObject, hitbox, Npc.NPCPlayer.CameraTransform.forward, Npc.NPCPlayer.Position, Npc.CurrentAIPlayerTarget.Position);

                    bool end = !Npc.CurrentAIPlayerTarget.IsAlive;

                    if (use_ammo)
                    {
                        Npc.AvailableWeapons[Npc.ItemHeld]--;
                        if (Npc.AvailableWeapons[Npc.ItemHeld] <= 0)
                        {
                            Npc.NPCPlayer.ReferenceHub.weaponManager.RpcReload(Npc.NPCPlayer.ReferenceHub.weaponManager.curWeapon);
                            Npc.AvailableWeapons[Npc.ItemHeld] = (int)Npc.NPCPlayer.ReferenceHub.weaponManager.weapons[Npc.NPCPlayer.ReferenceHub.weaponManager.curWeapon].maxAmmo;
                            if (end)
                            {
                                Npc.FireEvent(new Events.NPCTargetKilledEvent(Npc, Npc.CurrentAIPlayerTarget));
                            }
                            return Npc.NPCPlayer.ReferenceHub.weaponManager.weapons[Npc.NPCPlayer.ReferenceHub.weaponManager.curWeapon].reloadingTime;
                        }
                    }

                    if (end)
                    {
                        Npc.FireEvent(new Events.NPCTargetKilledEvent(Npc, Npc.CurrentAIPlayerTarget));
                        Npc.Stop();
                        Npc.CurrentAIPlayerTarget = null;
                        return 0f;
                    }
                }
                else
                {
                    return 0f;
                }
                return firerate_mul * Plugin.Instance.Config.NpcFireCooldownMultiplier * Npc.NPCPlayer.ReferenceHub.weaponManager._fireCooldown;
            }
            else
            {
                float cd = 0f;
                Npc.OnTargetLostBehaviour = Npc.TargetLostBehaviour.STOP;
                Npc.Follow(Npc.CurrentAIPlayerTarget);
                if (Vector3.Distance(Npc.CurrentAIPlayerTarget.Position, Npc.NPCPlayer.Position) <= 1.5f)
                {
                    if (Npc.NPCPlayer.Role.Is939())
                    {
                        Npc.NPCPlayer.GameObject.GetComponent<Scp939PlayerScript>().CallCmdShoot(Npc.CurrentAIPlayerTarget.GameObject);
                    }
                    else
                    {
                        switch (Npc.NPCPlayer.Role)
                        {
                            case RoleType.Scp106:
                                Npc.NPCPlayer.GameObject.GetComponent<Scp106PlayerScript>().CallCmdMovePlayer(Npc.CurrentAIPlayerTarget.GameObject, ServerTime.time);
                                cd = 2f;
                                break;

                            case RoleType.Scp173:
                                Npc.NPCPlayer.GameObject.GetComponent<Scp173PlayerScript>().CallCmdHurtPlayer(Npc.CurrentAIPlayerTarget.GameObject);
                                break;

                            case RoleType.Scp049:
                                Npc.CurrentAIPlayerTarget.Hurt(99999f, DamageTypes.Scp049, Npc.NPCPlayer.Nickname);
                                cd = PlayableScps.Scp049.KillCooldown;
                                break;

                            case RoleType.Scp0492:
                                Npc.NPCPlayer.GameObject.GetComponent<Scp049_2PlayerScript>().CallCmdShootAnim();
                                Npc.NPCPlayer.GameObject.GetComponent<Scp049_2PlayerScript>().CallCmdHurtPlayer(Npc.CurrentAIPlayerTarget.GameObject);
                                cd = 1f;
                                break;

                            case RoleType.Scp096:
                                Npc.CurrentAIPlayerTarget.Hurt(99999f, DamageTypes.Scp096, Npc.NPCPlayer.Nickname, Npc.NPCPlayer.Id);
                                break;
                        }
                    }
                    if (!Npc.CurrentAIPlayerTarget.IsAlive)
                    {
                        Npc.AttachedCoroutines.Add(MEC.Timing.CallDelayed(0.1f, () =>
                        {
                            Npc.FireEvent(new Events.NPCTargetKilledEvent(Npc, Npc.CurrentAIPlayerTarget));
                        }));

                        Npc.Stop();

                        Npc.CurrentAIPlayerTarget = null;

                        if (Npc.ProcessSCPLogic && Npc.NPCPlayer.Role == RoleType.Scp049)
                        {
                            Npc.AttachedCoroutines.Add(Timing.RunCoroutine(ReviveCoroutine(Npc, target)));
                            return PlayableScps.Scp049.TimeToRevive + 0.5f;
                        }
                    }
                }
                return cd;
            }
        }
    }

    public class SerializableVector3
    {
        public SerializableVector3()
        {
        }

        public SerializableVector3(Vector3 vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
        }

        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    public class NpcNodeSerializationInfo
    {
        public string Token { get; set; }
    }

    public class NpcEventSerializationInfo : NpcNodeSerializationInfo
    {
        public List<NpcNodeWithArgsSerializationInfo> Actions { get; set; }
    }

    public class NpcNodeWithArgsSerializationInfo : NpcNodeSerializationInfo
    {
        public Dictionary<string, string> Args { get; set; }
    }

    public enum AIMode
    {
        Legacy,
        Python
    }

    public class NpcSerializationInfo
    {
        public string Name { get; set; }
        public int Health { get; set; }
        public RoleType Role { get; set; }
        public float[] Scale { get; set; }

        [YamlMember(Alias = "item_held")]
        public ItemType ItemHeld { get; set; }

        public ItemType[] Inventory { get; set; }

        [YamlMember(Alias = "root_node")]
        public string RootNode { get; set; }

        [YamlMember(Alias = "god_mode")]
        public bool GodMode { get; set; }

        [YamlMember(Alias = "is_exclusive")]
        public bool IsExclusive { get; set; }

        [YamlMember(Alias = "process_scp_logic")]
        public bool ProcessScpLogic { get; set; }

        [YamlMember(Alias = "affect_summary")]
        public bool AffectSummary { get; set; }

        public NpcEventSerializationInfo[] Events { get; set; }

        [YamlMember(Alias = "ai_enabled")]
        public bool AiEnabled { get; set; }

        [YamlMember(Alias = "ai_mode")]
        public AIMode AiMode { get; set; }

        public NpcNodeWithArgsSerializationInfo[] Ai { get; set; }

        [YamlMember(Alias = "ai_scripts")]
        public string[] AiScripts { get; set; }
    }

    public class Utils
    {
        public static bool CompareWithType(string type, float a, float b)
        {
            switch (type)
            {
                case "equals":
                    return a.Equals(b);

                case "greater":
                    return a > b;

                case "less":
                    return a < b;

                case "greater_or_equals":
                    return a >= b;

                case "less_or_equals":
                    return a <= b;

                case "not_equals":
                    return !a.Equals(b);

                default:
                    return false;
            }
        }

        public static bool CheckItemType(string type, ItemType item)
        {
            switch (type)
            {
                case "keycard":
                    return item.IsKeycard();

                case "weapon":
                    return item.IsWeapon();

                default:
                    return item.ToString("g").Equals(type);
            }
        }
    }
}