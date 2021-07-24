using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;
using System;
using UnityEngine;

namespace NPCS.Commands
{
    [CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    internal class TalkCommand : ICommand
    {
        public string Command => "talk";

        public string[] Aliases => new string[] { "tlk" };

        public string Description => "Command which allows u to talk with NPCs";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                Player s = Player.Get(player.PlayerId);
                bool flag = false;
                foreach (NPCS.Npc obj_npc in NPCS.Npc.List)
                {
                    if (obj_npc.DisableDialogSystem || obj_npc.AIEnabled)
                    {
                        continue;
                    }
                    if (!obj_npc.IsActionLocked && !(obj_npc.IsExclusive && obj_npc.IsLocked))
                    {
                        if (Vector3.Distance(obj_npc.PlayerInstance.Position, s.Position) < 3f)
                        {
                            obj_npc.TalkWith(s);
                            flag = true;
                            break;
                        }
                    }
                    else
                    {
                        if (obj_npc.IsLocked && obj_npc.LockHandler == s)
                        {
                            s.SendConsoleMessage($"[{obj_npc.PlayerInstance.Nickname}] {Plugin.Instance.Config.TranslationAlreadyTalking}", "yellow");
                        }
                        else
                        {
                            s.SendConsoleMessage($"[{obj_npc.PlayerInstance.Nickname}] {Plugin.Instance.Config.TranslationNpcBusy}", "yellow");
                        }
                        response = null;

                        return false;
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