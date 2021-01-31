using Exiled.API.Features;
using MEC;
using NPCS.Talking;
using System;
using System.Collections.Generic;

namespace NPCS.Events
{
    public abstract class NPCEvent
    {
        public abstract string Name { get; }

        public NPCEvent(Npc npc, Player p)
        {
            NPC = npc;
            Player = p;
        }

        private IEnumerator<float> RunActions(List<KeyValuePair<NodeAction, Dictionary<string, string>>> acts)
        {
            foreach (KeyValuePair<NodeAction, Dictionary<string, string>> act in acts)
            {
                if (act.Key.IsExclusive)
                {
                    while (NPC.IsActionLocked)
                    {
                        yield return 0.0f;
                    }
                    NPC.IsActionLocked = true;
                }
                try
                {
                    act.Key.Process(NPC, Player, act.Value);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception during processing action {act.Key.Name}: {e}");
                }
                float dur = 0;
                try
                {
                    dur = float.Parse(act.Value["next_action_delay"].Replace('.', ','));
                }
                catch (Exception) { }
                if (act.Key.IsExclusive)
                {
                    NPC.IsActionLocked = false;
                }
                yield return Timing.WaitForSeconds(dur);
            }
        }

        public virtual void OnFired(Npc npc)
        {
        }

        public void FireActions(List<KeyValuePair<NodeAction, Dictionary<string, string>>> acts)
        {
            NPC.AttachedCoroutines.Add(Timing.RunCoroutine(RunActions(acts)));
        }

        public Npc NPC
        {
            get;
            set;
        }

        public Player Player
        {
            get;
            set;
        }
    }
}