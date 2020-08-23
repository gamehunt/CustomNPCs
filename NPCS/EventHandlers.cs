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
        public Plugin plugin;

        public EventHandlers(Plugin plugin) => this.plugin = plugin;

        public void OnRoundStart()
        {
            RoundSummaryFix.__npc_endRequested = false;
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            Npc[] npcs = UnityEngine.Object.FindObjectsOfType<Npc>();
            foreach (Npc npc in npcs)
            {
                npc.Kill(false);
            }
            RoundSummaryFix.__npc_endRequested = false;
            NavigationNode.Clear();
        }

        public void OnDied(DiedEventArgs ev)
        {
            Npc cmp = ev.Target.GameObject.GetComponent<Npc>();
            if (cmp != null)
            {
                NPCDiedEvent npc_ev = new NPCDiedEvent(cmp, ev.Killer);
                cmp.FireEvent(npc_ev);
                cmp.AttachedCoroutines.Add(Timing.RunCoroutine(Utils.CallOnUnlock(() => cmp.Kill(false), cmp)));
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
                        if (p.Health <= 0f)
                        {
                            NPCDiedEvent npc_ev = new NPCDiedEvent(component, ev.Thrower);
                            component.FireEvent(npc_ev);
                            component.AttachedCoroutines.Add(Timing.RunCoroutine(Utils.CallOnUnlock(() => component.Kill(true), component)));
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
            Npc intr = null;
            foreach (Npc component in UnityEngine.Object.FindObjectsOfType<Npc>())
            {
                component.FireEvent(new NPCDecontaminationEvent(intr, null));
            }
        }
    }
}