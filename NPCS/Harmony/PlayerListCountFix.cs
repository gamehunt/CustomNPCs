using HarmonyLib;
using Mirror.LiteNetLib4Mirror;
using NorthwoodLib;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.RefreshServerData))]
    internal class PlayerListCountFix
    {
        private static bool Prefix(ServerConsole __instance)
        {
            bool flag = true;
            byte b = 0;
            global::ServerConsole.RefreshEmailSetStatus();
            global::ServerConsole.RefreshToken(true);
            while (!global::ServerConsole._disposing)
            {
                b += 1;
                if (!flag && string.IsNullOrEmpty(global::ServerConsole.Password) && b < 15)
                {
                    if (b == 5 || b == 12 || global::ServerConsole.ScheduleTokenRefresh)
                    {
                        global::ServerConsole.RefreshToken(false);
                    }
                }
                else
                {
                    flag = false;
                    global::ServerConsole.Update = (global::ServerConsole.Update || b == 10);
                    string str = string.Empty;
                    try
                    {
                        int count = global::ServerConsole.NewPlayers.Count;
                        int num = 0;
                        List<Authenticator.AuthenticatorPlayerObject> list = ListPool<Authenticator.AuthenticatorPlayerObject>.Shared.Rent();
                        while (!global::ServerConsole.NewPlayers.IsEmpty)
                        {
                            num++;
                            if (num > count + 30)
                            {
                                break;
                            }
                            try
                            {
                                global::CharacterClassManager characterClassManager;
                                if (global::ServerConsole.NewPlayers.TryTake(out characterClassManager) && characterClassManager != null)
                                {
                                    list.Add(new Authenticator.AuthenticatorPlayerObject(characterClassManager.UserId, (characterClassManager.Connection == null || string.IsNullOrEmpty(characterClassManager.Connection.address)) ? "N/A" : characterClassManager.Connection.address, characterClassManager.RequestIp, characterClassManager.Asn, characterClassManager.AuthTokenSerial, characterClassManager.VacSession));
                                }
                            }
                            catch (Exception ex)
                            {
                                global::ServerConsole.AddLog("[VERIFICATION THREAD] Exception in New Player (inside of loop) processing: " + ex.Message, ConsoleColor.Gray);
                                global::ServerConsole.AddLog(ex.StackTrace, ConsoleColor.Gray);
                            }
                        }
                        str = global::JsonSerialize.ToJson<Authenticator.AuthenticatorPlayerObjects>(new Authenticator.AuthenticatorPlayerObjects(list));
                        ListPool<Authenticator.AuthenticatorPlayerObject>.Shared.Return(list);
                    }
                    catch (Exception ex2)
                    {
                        global::ServerConsole.AddLog("[VERIFICATION THREAD] Exception in New Players processing: " + ex2.Message, ConsoleColor.Gray);
                        global::ServerConsole.AddLog(ex2.StackTrace, ConsoleColor.Gray);
                    }
                    List<string> list2 = global::ServerConsole.Update ? new List<string>
                {
                    "ip=" + global::ServerConsole.Ip,
                    string.Concat(new object[]
                    {
                        "players=",
                        global::ServerConsole.PlayersAmount - Npc.Dictionary.Keys.Count,
                        "/",
                        global::CustomNetworkManager.slots
                    }),
                    "playersList=" + global::ServerConsole._verificationPlayersList,
                    "newPlayers=" + str,
                    "port=" + LiteNetLib4MirrorTransport.Singleton.port,
                    "pastebin=" + GameCore.ConfigFile.ServerConfig.GetString("serverinfo_pastebin_id", "7wV681fT"),
                    "gameVersion=" + GameCore.Version.VersionString,
                    "version=2",
                    "update=1",
                    "info=" + StringUtils.Base64Encode(__instance.RefreshServerNameSafe()).Replace('+', '-'),
                    "privateBeta=" + GameCore.Version.PrivateBeta.ToString(),
                    "staffRA=" + global::ServerStatic.PermissionsHandler.StaffAccess.ToString(),
                    "friendlyFire=" + global::ServerConsole.FriendlyFire.ToString(),
                    "geoblocking=" + (byte)global::CustomLiteNetLib4MirrorTransport.Geoblocking,
                    "modded=" + CustomNetworkManager.Modded.ToString(),
                    "cgs=" + (global::CustomNetworkManager.UsingCustomGamemode || global::ServerConsole.CustomGamemodeServerConfig).ToString(),
                    "whitelist=" + global::ServerConsole.WhiteListEnabled.ToString(),
                    "accessRestriction=" + global::ServerConsole.AccessRestriction.ToString(),
                    "emailSet=" + global::ServerConsole._emailSet.ToString(),
                    "enforceSameIp=" + global::ServerConsole.EnforceSameIp.ToString(),
                    "enforceSameAsn=" + global::ServerConsole.EnforceSameAsn.ToString()
                } : new List<string>
                {
                    "ip=" + global::ServerConsole.Ip,
                    string.Concat(new object[]
                    {
                        "players=",
                        global::ServerConsole.PlayersAmount - Npc.Dictionary.Keys.Count,
                        "/",
                        global::CustomNetworkManager.slots
                    }),
                    "newPlayers=" + str,
                    "port=" + LiteNetLib4MirrorTransport.Singleton.port,
                    "version=2",
                    "enforceSameIp=" + global::ServerConsole.EnforceSameIp.ToString(),
                    "enforceSameAsn=" + global::ServerConsole.EnforceSameAsn.ToString()
                };
                    if (!string.IsNullOrEmpty(global::ServerConsole.Password))
                    {
                        list2.Add("passcode=" + global::ServerConsole.Password);
                    }
                    global::ServerConsole.Update = false;
                    if (!Authenticator.AuthenticatorQuery.SendData(list2) && !global::ServerConsole._printedNotVerifiedMessage)
                    {
                        global::ServerConsole._printedNotVerifiedMessage = true;
                        global::ServerConsole.AddLog("Your server won't be visible on the public server list - (" + global::ServerConsole.Ip + ")", ConsoleColor.Red);
                        if (!global::ServerConsole._emailSet)
                        {
                            global::ServerConsole.AddLog("If you are 100% sure that the server is working, can be accessed from the Internet and YOU WANT TO MAKE IT PUBLIC, please set up your email in configuration file (\"contact_email\" value) and restart the server.", ConsoleColor.Red);
                        }
                        else
                        {
                            global::ServerConsole.AddLog("If you are 100% sure that the server is working, can be accessed from the Internet and YOU WANT TO MAKE IT PUBLIC please email following information:", ConsoleColor.Red);
                            global::ServerConsole.AddLog("- IP address of server (probably " + global::ServerConsole.Ip + ")", ConsoleColor.Red);
                            global::ServerConsole.AddLog("- is this static or dynamic IP address (most of home adresses are dynamic)", ConsoleColor.Red);
                            global::ServerConsole.AddLog("PLEASE READ rules for verified servers first: https://scpslgame.com/Verified_server_rules.pdf", ConsoleColor.Red);
                            global::ServerConsole.AddLog("send us that information to: server.verification@scpslgame.com (server.verification at scpslgame.com)", ConsoleColor.Red);
                            global::ServerConsole.AddLog("if you can't see the AT sign in console (in above line): server.verification AT scpslgame.com", ConsoleColor.Red);
                            global::ServerConsole.AddLog("email must be sent from email address set as \"contact_email\" in your config file (current value: " + GameCore.ConfigFile.ServerConfig.GetString("contact_email", "") + ").", ConsoleColor.Red);
                        }
                    }
                    else
                    {
                        global::ServerConsole._printedNotVerifiedMessage = true;
                    }
                }
                if (b >= 15)
                {
                    b = 0;
                }
                Thread.Sleep(5000);
                if (global::ServerConsole.ScheduleTokenRefresh || b == 0)
                {
                    global::ServerConsole.RefreshToken(false);
                }
            }
            return false;
        }
    }
}
