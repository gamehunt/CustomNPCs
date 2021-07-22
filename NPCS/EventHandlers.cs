using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using NPCS.Events;
using NPCS.Navigation;
using System.Collections.Generic;
using System;

namespace NPCS
{
    public class EventHandlers
    {


        public void OnRoundStart()
        {
            Timing.CallDelayed(0.1f, () =>
            {
                foreach (string mapping in Plugin.Instance.Config.InitialMappings)
                {
                    Methods.LoadNPCMappings(mapping);
                }
            });
        }

        public void OnWaitingForPlayers()
        {

        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
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
                Npc cmp = ev.Target.AsNPC();
                NPCDiedEvent npc_ev = new NPCDiedEvent(cmp, ev.Killer);
                cmp.FireEvent(npc_ev);
            }
        }

        public void OnHurt(HurtingEventArgs ev)
        {
            if (ev.Target.IsNPC())
            {
                Npc npc = ev.Target.AsNPC();
                npc.FireEvent(new NPCHurtEvent(npc, ev.Attacker));
            }
        }

        public void OnGrenadeExplosion(ExplodingGrenadeEventArgs ev)
        {
            foreach (Player p in ev.TargetToDamages.Keys)
            {
                if (p.IsNPC())
                {
                    Npc component = p.AsNPC();
                    if (!component.PlayerInstance.IsGodModeEnabled)
                    {
                        p.Hurt(ev.TargetToDamages[p], ev.Thrower, DamageTypes.Grenade);
                    }
                }
            }
        }

        public void OnEnteringPocketDim(EnteringPocketDimensionEventArgs ev)
        {
            if (ev.Player.IsNPC())
            {
                Npc npc = ev.Player.AsNPC();
                if (npc.PlayerInstance.IsGodModeEnabled)
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