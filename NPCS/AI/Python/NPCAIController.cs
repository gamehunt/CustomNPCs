using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NPCS.AI.Python
{
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

        public bool IsPlayerTargetValid(IronPython.Runtime.List filters = null)
        {
            return CurrentPlayerTarget != null && CurrentPlayerTarget.IsAlive && Npc.AIHelper.CheckPlayer(CurrentPlayerTarget, filters);
        }

        //Just for readability
        public void SetPlayerTarget(Player player)
        {
            CurrentPlayerTarget = player;
        }

        public void ResetPlayerTarget()
        {
            CurrentPlayerTarget = null;
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

        public bool IsGoing()
        {
            return Npc.CurrentAIRoomTarget != null;
        }

        private IEnumerator<float> ReviveCoroutine(Npc npc, Player target)
        {
            yield return Timing.WaitForSeconds(PlayableScps.Scp049.TimeToRevive);
            if (npc.PlayerInstance.IsAlive && target.IsDead)
            {
                target.Role = RoleType.Scp0492;
                yield return Timing.WaitForSeconds(0.3f);
                target.Position = npc.PlayerInstance.Position;
            }
        }

        public float Attack(Player target = null, int accuracy = 100, Dictionary<HitBoxType, int> hitboxes = null, bool use_ammo = false, float firerate_mul = 1f)
        {
            //TODO
            return 0.1f;
        }


    }
}