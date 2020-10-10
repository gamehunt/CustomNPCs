using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;
using System;
using UnityEngine;

namespace NPCS.Commands
{
    [CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    internal class ListAnswersCommand : ICommand
    {
        public string Command => "lansw";

        public string[] Aliases => new string[] { "lsa" };

        public string Description => "Command which allows u to get answers list from NPCs";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                Player s = Player.Get(player.PlayerId);
                bool flag = false;
                foreach (Npc obj_npc in Npc.List)
                {
                    if (Vector3.Distance(obj_npc.NPCPlayer.Position, s.Position) < 3f)
                    {
                        if (obj_npc.TalkingStates.ContainsKey(s))
                        {
                            obj_npc.TalkingStates[s].Send(obj_npc.Name, s);
                            flag = true;
                            break;
                        }
                    }
                }
                if (!flag)
                {
                    response = Plugin.Instance.Config.TranslationNpcNotFound;
                    return false;
                }
                response = null;
            }
            else
            {
                response = Plugin.Instance.Config.TranslationOnlyPlayers;
                return false;
            }

            return true;
        }
    }
}