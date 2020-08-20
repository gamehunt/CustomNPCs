using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using System;
using System.IO;

namespace NPCS.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class NPCCommand : ICommand
    {
        public string Command { get; } = "npc";

        public string[] Aliases { get; } = new string[] { "npc" };
        public string Description { get; } = "A master command of CustomNPCs plugin";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                Player s = Player.Get(player.PlayerId);
                if (!s.CheckPermission("npc.all"))
                {
                    response = "Access denied!";
                }
                if (!Round.IsStarted)
                {
                    response = "Round is not started!";
                    return false;
                }
                switch (arguments.At(0))
                {
                    case "create":

                        if (arguments.Count == 1)
                        {
                            Npc.CreateNPC(s.Position, s.Rotations, "default_npc.yml");
                        }
                        else if (arguments.Count == 2)
                        {
                            Npc.CreateNPC(s.Position, s.Rotations, RoleType.Scientist, ItemType.None, arguments.At(1));
                        }
                        else if (arguments.Count == 3)
                        {
                            Npc.CreateNPC(s.Position, s.Rotations, (RoleType)int.Parse(arguments.At(2)), ItemType.None, arguments.At(1));
                        }
                        else if (arguments.Count == 4)
                        {
                            Npc.CreateNPC(s.Position, s.Rotations, (RoleType)int.Parse(arguments.At(2)), (ItemType)int.Parse(arguments.At(3)), arguments.At(1));
                        }
                        else if (arguments.Count == 5)
                        {
                            Npc.CreateNPC(s.Position, s.Rotations, (RoleType)int.Parse(arguments.At(2)), (ItemType)int.Parse(arguments.At(3)), arguments.At(1), arguments.At(4));
                        }
                        response = "NPC created";
                        break;

                    case "load":
                        if (arguments.Count < 2)
                        {
                            response = "You need to provide path to file!";
                            return false;
                        }
                        Npc.CreateNPC(s.Position, s.Rotations, arguments.At(1));
                        response = "NPC loaded";
                        break;

                    case "save":
                        if (arguments.Count < 3)
                        {
                            response = "You need to provide npc id and path to file!";
                            break;
                        }
                        NPCComponent[] ___npcs = UnityEngine.Object.FindObjectsOfType<NPCComponent>();
                        Npc __obj_npc = Npc.FromComponent(___npcs[int.Parse(arguments.At(1))]);
                        __obj_npc.Serialize(arguments.At(2));
                        response = "NPC saved";
                        break;

                    case "list":
                        NPCComponent[] __npcs = UnityEngine.Object.FindObjectsOfType<NPCComponent>();
                        int id = 0;
                        foreach (NPCComponent npc in __npcs)
                        {
                            Npc obj_npc = Npc.FromComponent(npc);
                            s.RemoteAdminMessage($"{id} | {obj_npc.Name} | {Path.GetFileName(obj_npc.RootNode.NodeFile)}", true, Plugin.Instance.Name);
                            id++;
                        }
                        response = "List ended";
                        break;

                    case "clean":
                        NPCComponent[] npcs = UnityEngine.Object.FindObjectsOfType<NPCComponent>();
                        foreach (NPCComponent npc in npcs)
                        {
                            Npc obj_npc = Npc.FromComponent(npc);
                            obj_npc.Kill(false);
                        }
                        response = "NPCs cleaned";
                        break;

                    case "remove":
                        if (arguments.Count > 1)
                        {
                            NPCComponent[] _npcs = UnityEngine.Object.FindObjectsOfType<NPCComponent>();
                            Npc obj_npc = Npc.FromComponent(_npcs[int.Parse(arguments.At(1))]);
                            obj_npc.Kill(false);
                            response = "NPC removed!";
                        }
                        else
                        {
                            response = "You need to provide NPC's id!";
                            return false;
                        }
                        break;

                    case "god":
                        if (arguments.Count <= 2)
                        {
                            response = "You need to provide npc id and godmode value!";
                            return false;
                        }
                        NPCComponent[] ___npcs1 = UnityEngine.Object.FindObjectsOfType<NPCComponent>();
                        Player.Get(___npcs1[int.Parse(arguments.At(1))].gameObject).IsGodModeEnabled = bool.Parse(arguments.At(2));
                        response = "God-Mode switched";
                        break;

                    default:
                        response = "Unknown sub-command!";
                        break;
                }
            }
            else
            {
                response = "Only players can use this!";
            }
            return true;
        }
    }
}