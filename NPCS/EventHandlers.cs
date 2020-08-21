using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using NPCS.Events;
using NPCS.Harmony;
using System;
using System.Collections.Generic;

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
            NPCComponent[] npcs = UnityEngine.Object.FindObjectsOfType<NPCComponent>();
            foreach (NPCComponent npc in npcs)
            {
                Npc obj_npc = Npc.FromComponent(npc);
                obj_npc.Kill(false);
            }
            RoundSummaryFix.__npc_endRequested = false;
        }

        private IEnumerator<float> CallOnUnlock(Action act, Npc locked)
        {
            while (locked.IsActionLocked)
            {
                yield return 0f;
            }
            act.Invoke();
        }

        public void OnDied(DiedEventArgs ev)
        {
            NPCComponent cmp = ev.Target.GameObject.GetComponent<NPCComponent>();
            if (cmp != null)
            {
                Npc npc = Npc.FromComponent(cmp);
                NPCDiedEvent npc_ev = new NPCDiedEvent(npc,ev.Killer);
                npc.FireEvent(npc_ev);
                npc.NPCComponent.attached_coroutines.Add(Timing.RunCoroutine(CallOnUnlock(() => npc.Kill(false), npc)));
            }
        }

        public void OnGrenadeExplosion(ExplodingGrenadeEventArgs ev)
        {
            foreach (Player p in ev.TargetToDamages.Keys)
            {
                NPCComponent component = p.GameObject.GetComponent<NPCComponent>();
                if (component != null)
                {
                    Npc obj_npc = Npc.FromComponent(component);
                    if (!Player.Get(obj_npc.GameObject).IsGodModeEnabled)
                    {
                        p.Health -= ev.TargetToDamages[p];
                        if (p.Health <= 0f)
                        {

                            obj_npc.Kill(true);
                        }
                    }
                }
            }
        }
    }
}