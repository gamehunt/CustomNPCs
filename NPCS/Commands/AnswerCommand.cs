using CommandSystem;
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
                    NPCComponent[] npcs = UnityEngine.Object.FindObjectsOfType<NPCComponent>();
                    foreach (NPCComponent npc in npcs)
                    {
                        Npc obj_npc = Npc.FromComponent(npc);
                        if (Vector3.Distance(npc.transform.position, s.Position) < 3f)
                        {
                            obj_npc.HandleAnswer(s, arguments.At(0));
                        }
                    }
                    response = null;
                }
                else
                {
                    response = "You must provide answer number!";
                    return false;
                }
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