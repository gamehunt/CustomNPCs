﻿using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;
using System;
using UnityEngine;

namespace NPCS.Commands
{
    [CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    internal class AnswerCommand : ICommand
    {
        public string Command => "answer";

        public string[] Aliases => new string[] { "answ" };

        public string Description => "Command that allow u to answer to NPC";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                Player s = Player.Get(player.PlayerId);
                if (arguments.Count == 1)
                {
                    bool flag = false;
                    foreach (Npc npc in Npc.List)
                    {
                        if (Vector3.Distance(npc.NPCPlayer.Position, s.Position) < 3f)
                        {
                            npc.HandleAnswer(s, arguments.At(0));
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        response = Plugin.Instance.Config.TranslationNpcNotFound;
                    }
                    else
                    {
                        response = null;
                    }
                }
                else
                {
                    response = Plugin.Instance.Config.TranslationAnswerNumber;
                    return false;
                }
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