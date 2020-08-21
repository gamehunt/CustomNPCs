using Exiled.API.Features;
using MEC;
using NPCS.Talking;
using System.Collections.Generic;
using UnityEngine;

namespace NPCS
{
    //This component contains critical information about NPC
    internal class NPCComponent : MonoBehaviour
    {
        public TalkNode root_node;
        public Dictionary<Player, TalkNode> talking_states = new Dictionary<Player, TalkNode>();

        public List<CoroutineHandle> attached_coroutines = new List<CoroutineHandle>();

        public Npc.MovementDirection curDir;

        public bool action_locked = false;
        public bool locked = false;

        public bool is_exclusive = false;

        private void OnDestroy()
        {
            Log.Debug("Destroying component", Plugin.Instance.Config.VerboseOutput);
            Timing.KillCoroutines(attached_coroutines);
        }
    }
}