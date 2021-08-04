using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using NPCS.Navigation;
using System.Collections.Generic;
using System;

namespace NPCS
{
    public class EventHandlers
    {


        public void OnRoundStart()
        {
            Timing.CallDelayed(0.1f, () =>
            {
                foreach (string mapping in Plugin.Instance.Config.InitialMappings)
                {
                    Methods.LoadNPCMappings(mapping);
                }
            });
        }

        public void OnWaitingForPlayers()
        {

        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            NavigationNode.Clear();
        }

        public void OnWarheadStart(StartingEventArgs ev)
        {
            foreach (Npc npc in Npc.List)
            {
                npc.FireEvent("WarheadStartEvent", new Dictionary<string, object>() { });
            }
        }

        public void OnTeamRespawning(RespawningTeamEventArgs ev)
        {
            foreach (Npc npc in Npc.List)
            {
                npc.FireEvent("HurtEvent", new Dictionary<string, object>() { { "team", ev.NextKnownTeam } });
            }
        }

        public void OnHurt(HurtingEventArgs ev)
        {
            if (ev.Target.IsNPC())
            {
                Npc npc = ev.Target.AsNPC();
                npc.FireEvent("HurtEvent", new Dictionary<string, object>() { {"damage" , ev.Amount}});
            }
        }

        public void OnEnteringPocketDim(EnteringPocketDimensionEventArgs ev)
        {
            if (ev.Player.IsNPC())
            {
                Npc npc = ev.Player.AsNPC();
                if (npc.PlayerInstance.IsGodModeEnabled)
                {
                    ev.IsAllowed = false;
                }
            }
        }

        public void OnDecontamination(DecontaminatingEventArgs ev)
        {
            foreach (Npc component in Npc.List)
            {
                component.FireEvent("DecontaminationEvent", new Dictionary<string, object>() { });
            }
        }
    }
}