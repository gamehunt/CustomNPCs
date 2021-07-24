using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;
using System;
using UnityEngine;

namespace NPCS.Commands
{
    [CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    internal class EndCommand : ICommand
    {
        public string Command => "end";

        public string[] Aliases => new string[] { "end" };

        public string Description => "Command which allows u to end talk with NPCs";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                Player s = Player.Get(player.PlayerId);
                bool flag = false;
                foreach (NPCS.Npc obj_npc in NPCS.Npc.List)
                {
                    if (Vector3.Distance(obj_npc.PlayerInstance.Position, s.Position) < 3f)
                    {
                        if (obj_npc.IsLocked && obj_npc.LockHandler == s)
                        {
                            obj_npc.IsLocked = false;
                            obj_npc.LockHandler = null;
                        }
                        obj_npc.TalkingStates.Remove(s);
                        s.SendConsoleMessage($"ended talk with {obj_npc.PlayerInstance.Nickname}", "yellow");
                        flag = true;
                        break;
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