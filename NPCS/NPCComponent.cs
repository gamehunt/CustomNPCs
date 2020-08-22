using Exiled.API.Features;
using MEC;
using NPCS.Navigation;
using NPCS.Talking;
using System.Collections.Generic;
using UnityEngine;

namespace NPCS
{
    //This component contains critical information about NPC. It's only one per NPC
    internal class NPCComponent : MonoBehaviour
    {
        public TalkNode root_node;
        public Dictionary<Player, TalkNode> talking_states = new Dictionary<Player, TalkNode>();

        public List<CoroutineHandle> attached_coroutines = new List<CoroutineHandle>();
        public List<CoroutineHandle> movement_coroutines = new List<CoroutineHandle>();

        public Dictionary<string, Dictionary<NodeAction, Dictionary<string, string>>> attached_events = new Dictionary<string, Dictionary<NodeAction, Dictionary<string, string>>>(); //Horrible

        public Queue<NavigationNode> nav_queue = new Queue<NavigationNode>();
        public NavigationNode nav_current_target = null;

        public Player follow_target = null;

        public Npc.MovementDirection curDir;

        public bool action_locked = false;
        public Player lock_handler = null;
        public bool locked = false;

        public bool is_exclusive = false;

        public float speed = 2f;

        private void OnDestroy()
        {
            Log.Debug("Destroying NPC component", Plugin.Instance.Config.VerboseOutput);
            Timing.KillCoroutines(movement_coroutines);
            Timing.KillCoroutines(attached_coroutines);
        }
    }
}