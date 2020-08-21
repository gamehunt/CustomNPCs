using Exiled.API.Features;
using MEC;
using NPCS.Talking;
using System;
using System.Collections.Generic;
using System.Text;

namespace NPCS.Events
{
    abstract class NPCEvent
    {
        public abstract string Name { get; }

        public NPCEvent(Npc npc,Player p)
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
                act.Process(NPC, Player, acts[act]);
                yield return Timing.WaitForSeconds(float.Parse(acts[act]["next_action_delay"].Replace('.', ',')));
            }
            NPC.IsActionLocked = false;
        }

        public void FireActions(Dictionary<NodeAction,Dictionary<string,string>> acts)
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
