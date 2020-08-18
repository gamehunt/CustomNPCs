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
        public TalkNode __node;
        public Dictionary<Player, TalkNode> TalkingStates = new Dictionary<Player, TalkNode>();
        public CoroutineHandle coro;

        private void OnDestroy()
        {
            Log.Debug("Destroying component", Plugin.Instance.Config.VerboseOutput);
            Timing.KillCoroutines(coro);
        }
    }
}