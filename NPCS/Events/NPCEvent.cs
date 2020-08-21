using Exiled.API.Features;
using MEC;
using NPCS.Talking;
using System.Collections.Generic;
using System;

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
            while (NPC.IsActionLocked)
            {
                yield return 0f;
            }
            NPC.IsActionLocked = true;
            foreach (NodeAction act in acts.Keys)
            {
                try
                {
                    act.Process(NPC, Player, acts[act]);
                }catch(Exception e)
                {
                    Log.Error($"Exception during processing action {act.Name}: {e}");
                }
                yield return Timing.WaitForSeconds(float.Parse(acts[act]["next_action_delay"].Replace('.', ',')));
            }
            NPC.IsActionLocked = false;
        }

        public void FireActions(Dictionary<NodeAction, Dictionary<string, string>> acts)
        {
            NPC.NPCComponent.attached_coroutines.Add(Timing.RunCoroutine(RunActions(acts)));
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