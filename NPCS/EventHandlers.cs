using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using NPCS.Events;
using NPCS.Harmony;
using NPCS.Navigation;
using System.Collections.Generic;
using System;

namespace NPCS
{
    public class EventHandlers
    {


        public void OnRoundStart()
        {
            RoundSummaryFix.__npc_endRequested = false;
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
            ServerConsole.singleton.NameFormatter.Commands["player_count"] = delegate (List<string> args)
            {
                return (global::PlayerManager.players.Count - Npc.Dictionary.Count).ToString();
            };
            ServerConsole.singleton.NameFormatter.Commands["full_player_count"] = delegate (List<string> args)
            {
                int count = global::PlayerManager.players.Count - Npc.Dictionary.Count;
                if (count != global::CustomNetworkManager.TypedSingleton.ReservedMaxPlayers)
                {
                    return string.Format("{0}/{1}", count, global::CustomNetworkManager.TypedSingleton.ReservedMaxPlayers);
                }
                int count2 = args.Count;
                if (count2 == 1)
                {
                    return "FULL";
                }
                if (count2 != 2)
                {
                    throw new ArgumentOutOfRangeException("args", args, "Invalid arguments. Use: full_player_count OR full_player_count,[full]");
                }
                return ServerConsole.singleton.NameFormatter.ProcessExpression(args[1]);
            };
            if (Plugin.Instance.Config.GenerateNavigationGraph)
            {
                Timing.CallDelayed(0.1f, () =>
                {
                    Methods.GenerateNavGraph();
                });
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
                Npc cmp = Npc.Get(ev.Target);
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
                Npc npc = Npc.Get(ev.Target);
                npc.FireEvent(new NPCHurtEvent(npc, ev.Attacker));
            }
        }

        public void OnGrenadeExplosion(ExplodingGrenadeEventArgs ev)
        {
            foreach (Player p in ev.TargetToDamages.Keys)
            {
                if (p.IsNPC())
                {
                    Npc component = Npc.Get(p);
                    if (!component.NPCPlayer.IsGodModeEnabled)
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
                Npc npc = Npc.Get(ev.Player);
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