using Exiled.API.Features;
using MEC;
using NPCS.Talking;
using System;
using System.Collections.Generic;

namespace NPCS.Events
{
    internal abstract class NPCEvent
    {
        public abstract string Name { get; }

        public NPCEvent(Npc npc, Player p)
        {
            NPC = npc;
            Player = p;
        }

        private IEnumerator<float> RunActions(Dictionary<NodeAction, Dictionary<string, string>> acts)
        {
            NPC.IsActionLocked = true;
            foreach (NodeAction act in acts.Keys)
            {
                try
                {
                    act.Process(NPC, Player, acts[act]);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception during processing action {act.Name}: {e}");
                }
                float dur = 0;
                try
                {
                    dur = float.Parse(acts[act]["next_action_delay"].Replace('.', ','));
                }
                catch (Exception) { }
                yield return Timing.WaitForSeconds(dur);
            }
            NPC.IsActionLocked = false;
        }

        public virtual void OnFired(Npc npc)
        {
        }

        public void FireActions(Dictionary<NodeAction, Dictionary<string, string>> acts)
        {
            if (!NPC.IsActionLocked)
            {
                NPC.AttachedCoroutines.Add(Timing.RunCoroutine(RunActions(acts)));
            }
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