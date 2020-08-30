using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using NPCS.Events;
using NPCS.Harmony;
using NPCS.Navigation;
using System.Collections.Generic;

namespace NPCS
{
    public class EventHandlers
    {
        public void OnRoundStart()
        {
            RoundSummaryFix.__npc_endRequested = false;
            Timing.CallDelayed(0.5f, () =>
            {
                foreach (string mapping in Plugin.Instance.Config.InitialMappings)
                {
                    Npc.LoadNPCMappings(mapping);
                }
            });
        }

        public void OnWaitingForPlayers()
        {
            Methods.GenerateNavGraph();
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

        public void OnDying(DyingEventArgs ev)
        {
            Npc cmp = ev.Target.GameObject.GetComponent<Npc>();
            if (cmp != null)
            {
                NPCDiedEvent npc_ev = new NPCDiedEvent(cmp, ev.Killer);
                cmp.FireEvent(npc_ev);
                cmp.Kill(ev.HitInformation.GetDamageType() != DamageTypes.RagdollLess);
                ev.IsAllowed = false;
            }
        }

        public void OnHurt(HurtingEventArgs ev)
        {
            Npc npc = ev.Target.GameObject.GetComponent<Npc>();
            if (npc != null)
            {
                npc.FireEvent(new NPCHurtEvent(npc, ev.Attacker));
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