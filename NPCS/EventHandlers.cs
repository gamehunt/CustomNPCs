using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using NPCS.Events;
using NPCS.Harmony;
using NPCS.Navigation;
using System.Collections.Generic;
using UnityEngine;

namespace NPCS
{
    public class EventHandlers
    {
        public void OnRoundStart()
        {
            RoundSummaryFix.__npc_endRequested = false;
        }

        public void OnWaitingForPlayers()
        {
            Log.Info("[NAV] Generating navigation graph...");
            foreach (Room r in Map.Rooms)
            {
                NavigationNode node = NavigationNode.Create(r.Position, $"AUTO_Room_{r.Name}".Replace(' ', '_'));
                foreach (Door d in r.GetDoors())
                {
                    if (d.gameObject.transform.position == Vector3.zero)
                    {
                        continue;
                    }
                    NavigationNode new_node = NavigationNode.Create(d.gameObject.transform.position, $"AUTO_Door_{(d.DoorName.IsEmpty() ? d.gameObject.transform.position.ToString() : d.DoorName)}".Replace(' ', '_'));
                    if (new_node == null)
                    {
                        new_node = NavigationNode.AllNodes[$"AUTO_Door_{(d.DoorName.IsEmpty() ? d.gameObject.transform.position.ToString() : d.DoorName)}".Replace(' ', '_')];
                    }
                    else
                    {
                        new_node.AttachedDoor = d;
                    }
                    node.LinkedNodes.Add(new_node);
                    new_node.LinkedNodes.Add(node);
                    Log.Debug($"[NAV] Linked door {new_node.Name} node to room {r.Name}", Plugin.Instance.Config.VerboseOutput);
                }
            }
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            List<Npc> list = Npc.List;
            foreach (Npc npc in list)
            {
                npc.Kill(false);
            }
            RoundSummaryFix.__npc_endRequested = false;
            NavigationNode.Clear();
        }

        public void OnWarheadStart(StartingEventArgs ev)
        {
            List<Npc> list = Npc.List;
            foreach (Npc npc in list)
            {
                NPCWarheadStartedEvent nev = new NPCWarheadStartedEvent(npc, ev.Player);
                npc.AttachedCoroutines.Add(Timing.CallDelayed(0.1f, () => npc.FireEvent(nev)));
            }
        }

        public void OnTeamRespawning(RespawningTeamEventArgs ev)
        {
            List<Npc> list = Npc.List;
            foreach (Npc npc in list)
            {
                NPCTeamRespawnEvent nev = new NPCTeamRespawnEvent(npc, null, ev.NextKnownTeam);
                npc.FireEvent(nev);
            }
        }

        public void OnDied(DiedEventArgs ev)
        {
            Npc cmp = ev.Target.GameObject.GetComponent<Npc>();
            if (cmp != null)
            {
                NPCDiedEvent npc_ev = new NPCDiedEvent(cmp, ev.Killer);
                cmp.FireEvent(npc_ev);
                cmp.Kill(false);
            }
        }

        public void OnHurt(HurtingEventArgs ev)
        {
            Npc npc = ev.Target.GameObject.GetComponent<Npc>();
            if (npc != null)
            {
                npc.FireEvent(new NPCHurtEvent(npc, ev.Attacker));
                npc.NPCPlayer.Health -= ev.Amount;
                if (npc.NPCPlayer.Health <= 0f)
                {
                    NPCDiedEvent npc_ev = new NPCDiedEvent(npc, ev.Attacker);
                    npc.FireEvent(npc_ev);
                    npc.Kill(true);
                }
            }
        }

        public void OnGrenadeExplosion(ExplodingGrenadeEventArgs ev)
        {
            foreach (Player p in ev.TargetToDamages.Keys)
            {
                Npc component = p.GameObject.GetComponent<Npc>();
                if (component != null)
                {
                    if (!component.NPCPlayer.IsGodModeEnabled)
                    {
                        p.Health -= ev.TargetToDamages[p];
                        component.FireEvent(new NPCHurtEvent(component, ev.Thrower));
                        if (p.Health <= 0f)
                        {
                            NPCDiedEvent npc_ev = new NPCDiedEvent(component, ev.Thrower);
                            component.FireEvent(npc_ev);
                            component.Kill(true);
                        }
                    }
                }
            }
        }

        public void OnEnteringPocketDim(EnteringPocketDimensionEventArgs ev)
        {
            Npc npc = ev.Player.GameObject.GetComponent<Npc>();
            if (npc != null)
            {
                if (npc.NPCPlayer.IsGodModeEnabled)
                {
                    ev.IsAllowed = false;
                }
            }
        }

        public void OnDecontamination(DecontaminatingEventArgs ev)
        {
            foreach (Npc component in Npc.List)
            {
                component.FireEvent(new NPCDecontaminationEvent(component, null));
            }
        }
    }
}