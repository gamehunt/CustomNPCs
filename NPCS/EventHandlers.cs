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

        public void OnRACMD(SendingRemoteAdminCommandEventArgs ev)
        {
            switch (ev.Name)
            {
                case "npc_create":
                    ev.IsAllowed = false;
                    if (!ev.Sender.CheckPermission("npc.all"))
                    {
                        ev.ReplyMessage = "Access denied!";
                        ev.Success = false;
                        break;
                    }
                    if (ev.Arguments.Count == 0)
                    {
                        Npc.CreateNPC(ev.Sender.Position, ev.Sender.Rotations, "default_npc.yml");
                    }
                    else if (ev.Arguments.Count == 1)
                    {
                        Npc.CreateNPC(ev.Sender.Position, ev.Sender.Rotations, RoleType.Scientist, ItemType.None, ev.Arguments[0]);
                    }
                    else if (ev.Arguments.Count == 2)
                    {
                        Npc.CreateNPC(ev.Sender.Position, ev.Sender.Rotations, (RoleType)int.Parse(ev.Arguments[1]), ItemType.None, ev.Arguments[0]);
                    }
                    else if (ev.Arguments.Count == 3)
                    {
                        Npc.CreateNPC(ev.Sender.Position, ev.Sender.Rotations, (RoleType)int.Parse(ev.Arguments[1]), (ItemType)int.Parse(ev.Arguments[2]), ev.Arguments[0]);
                    }
                    else if (ev.Arguments.Count == 4)
                    {
                        Npc.CreateNPC(ev.Sender.Position, ev.Sender.Rotations, (RoleType)int.Parse(ev.Arguments[1]), (ItemType)int.Parse(ev.Arguments[2]), ev.Arguments[0], ev.Arguments[3]);
                    }
                    break;

                case "npc_clean":
                    ev.IsAllowed = false;
                    if (!ev.Sender.CheckPermission("npc.all"))
                    {
                        ev.ReplyMessage = "Access denied!";
                        ev.Success = false;
                        break;
                    }
                    NPCComponent[] npcs = UnityEngine.Object.FindObjectsOfType<NPCComponent>();
                    foreach (NPCComponent npc in npcs)
                    {
                        Npc obj_npc = Npc.FromComponent(npc);
                        obj_npc.Kill(false);
                    }
                    break;

                case "npc_remove":
                    ev.IsAllowed = false;
                    if (!ev.Sender.CheckPermission("npc.all"))
                    {
                        ev.ReplyMessage = "Access denied!";
                        ev.Success = false;
                        break;
                    }
                    if (ev.Arguments.Count > 0)
                    {
                        NPCComponent[] _npcs = UnityEngine.Object.FindObjectsOfType<NPCComponent>();
                        Npc obj_npc = Npc.FromComponent(_npcs[int.Parse(ev.Arguments[0])]);
                        obj_npc.Kill(false);
                    }
                    else
                    {
                        ev.ReplyMessage = "You need to provide NPC's id!";
                    }
                    break;

                case "npc_list":
                    ev.IsAllowed = false;
                    if (!ev.Sender.CheckPermission("npc.all"))
                    {
                        ev.ReplyMessage = "Access denied!";
                        ev.Success = false;
                        break;
                    }
                    NPCComponent[] __npcs = UnityEngine.Object.FindObjectsOfType<NPCComponent>();
                    int id = 0;
                    foreach (NPCComponent npc in __npcs)
                    {
                        Npc obj_npc = Npc.FromComponent(npc);
                        ev.Sender.RemoteAdminMessage($"{id} | {obj_npc.Name} | {Path.GetFileName(obj_npc.RootNode.NodeFile)}", true, plugin.Name);
                        id++;
                    }
                    break;

                case "npc_load":
                    ev.IsAllowed = false;
                    if (!ev.Sender.CheckPermission("npc.all"))
                    {
                        ev.ReplyMessage = "Access denied!";
                        ev.Success = false;
                        break;
                    }
                    if (ev.Arguments.Count < 1)
                    {
                        ev.ReplyMessage = "You need to provide path to file!";
                        ev.Success = false;
                        break;
                    }
                    Npc.CreateNPC(ev.Sender.Position, ev.Sender.Rotations, ev.Arguments[0]);
                    break;

                case "npc_save":
                    ev.IsAllowed = false;
                    if (!ev.Sender.CheckPermission("npc.all"))
                    {
                        ev.ReplyMessage = "Access denied!";
                        ev.Success = false;
                        break;
                    }
                    if (ev.Arguments.Count < 2)
                    {
                        ev.ReplyMessage = "You need to provide npc id and path to file!";
                        ev.Success = false;
                        break;
                    }
                    NPCComponent[] ___npcs = UnityEngine.Object.FindObjectsOfType<NPCComponent>();
                    Npc __obj_npc = Npc.FromComponent(___npcs[int.Parse(ev.Arguments[0])]);
                    __obj_npc.Serialize(ev.Arguments[1]);
                    break;

                case "npc_god":
                    ev.IsAllowed = false;
                    if (!ev.Sender.CheckPermission("npc.all"))
                    {
                        ev.ReplyMessage = "Access denied!";
                        ev.Success = false;
                        break;
                    }
                    if (ev.Arguments.Count < 2)
                    {
                        ev.ReplyMessage = "You need to provide npc id and godmode value!";
                        ev.Success = false;
                        break;
                    }
                    NPCComponent[] ___npcs1 = UnityEngine.Object.FindObjectsOfType<NPCComponent>();
                    Player.Get(___npcs1[int.Parse(ev.Arguments[0])].gameObject).IsGodModeEnabled = bool.Parse(ev.Arguments[1]);
                    break;
            }
        }

        public void OnCMD(SendingConsoleCommandEventArgs ev)
        {
            if (ev.Name == "talk")
            {
                ev.IsAllowed = false;
                bool flag = false;
                NPCComponent[] npcs = UnityEngine.Object.FindObjectsOfType<NPCComponent>();
                foreach (NPCComponent npc in npcs)
                {
                    Npc obj_npc = Npc.FromComponent(npc);
                    if (Vector3.Distance(npc.transform.position, ev.Player.Position) < 3f)
                    {
                        //ev.ReturnMessage = $"Talking to {obj_npc.GameObject.GetComponent<NicknameSync>().Network_myNickSync}";
                        obj_npc.TalkWith(ev.Player);
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    ev.ReturnMessage = "NPCs not found!";
                }
            }
            else if (ev.Name == "answ")
            {
                ev.IsAllowed = false;
                if (ev.Arguments.Count == 1)
                {
                    NPCComponent[] npcs = UnityEngine.Object.FindObjectsOfType<NPCComponent>();
                    foreach (NPCComponent npc in npcs)
                    {
                        Npc obj_npc = Npc.FromComponent(npc);
                        if (Vector3.Distance(npc.transform.position, ev.Player.Position) < 3f)
                        {
                            obj_npc.HandleAnswer(ev.Player, ev.Arguments[0]);
                        }
                    }
                }
                else
                {
                    ev.ReturnMessage = "You must provide answer number!";
                }
            }
        }
    }
}