using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using NPCS.Events;
using NPCS.Harmony;
using NPCS.Navigation;

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
            if (Plugin.Instance.Config.GenerateNavigationGraph)
            {
                Methods.GenerateNavGraph();
            }
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            foreach (Npc npc in Npc.List)
            {
                    npc.Kill(false);
            }
            RoundSummaryFix.__npc_endRequested = false;
            NavigationNode.Clear();
        }

        public void OnWarheadStart(StartingEventArgs ev)
        {
            foreach (Npc npc in Npc.List)
            {
                NPCWarheadStartedEvent nev = new NPCWarheadStartedEvent(npc, ev.Player);
                npc.AttachedCoroutines.Add(Timing.CallDelayed(0.1f, () => npc.FireEvent(nev)));
            }
        }

        public void OnTeamRespawning(RespawningTeamEventArgs ev)
        {
            foreach (Npc npc in Npc.List)
            {
                NPCTeamRespawnEvent nev = new NPCTeamRespawnEvent(npc, null, ev.NextKnownTeam);
                npc.FireEvent(nev);
            }
        }

        public void OnDying(DyingEventArgs ev)
        {
            
            if (ev.Target.IsNPC())
            {
                Npc cmp = Npc.Dictionary[ev.Target.GameObject];
                NPCDiedEvent npc_ev = new NPCDiedEvent(cmp, ev.Killer);
                cmp.FireEvent(npc_ev);
                cmp.Kill(ev.HitInformation.GetDamageType() != DamageTypes.RagdollLess);
                ev.IsAllowed = false;
            }
        }

        public void OnHurt(HurtingEventArgs ev)
        {
            
            if (ev.Target.IsNPC())
            {
                Npc npc = Npc.Dictionary[ev.Target.GameObject];
                npc.FireEvent(new NPCHurtEvent(npc, ev.Attacker));
            }
        }

        public void OnGrenadeExplosion(ExplodingGrenadeEventArgs ev)
        {
            foreach (Player p in ev.TargetToDamages.Keys)
            {
                
                if (p.IsNPC())
                {
                    Npc component = Npc.Dictionary[p.GameObject];
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
            if (ev.Player.IsNPC())
            {
                Npc npc = Npc.Dictionary[ev.Player.GameObject];
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