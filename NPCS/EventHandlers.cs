using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Permissions.Extensions;
using NPCS.Harmony;
using System.IO;
using UnityEngine;

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

        public void OnDied(DiedEventArgs ev)
        {
            NPCComponent cmp = ev.Target.GameObject.GetComponent<NPCComponent>();
            if (cmp != null)
            {
                Npc npc = Npc.FromComponent(cmp);
                npc.Kill(false);
            }
        }
    }
}