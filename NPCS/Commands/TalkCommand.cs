using CommandSystem;
using System;
using RemoteAdmin;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace NPCS.Commands
{
    [CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    class TalkCommand : ICommand
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
                NPCComponent[] npcs = UnityEngine.Object.FindObjectsOfType<NPCComponent>();
                foreach (NPCComponent npc in npcs)
                {
                    Npc obj_npc = Npc.FromComponent(npc);
                    if (Vector3.Distance(npc.transform.position, s.Position) < 3f)
                    {
                        obj_npc.TalkWith(s);
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    response = "NPCs not found!";
                    return false;
                }
                response = null;
            }
            else
            {
                response = "Only players can use this!";
                return false;
            }

            return true;
        }
    }
}
