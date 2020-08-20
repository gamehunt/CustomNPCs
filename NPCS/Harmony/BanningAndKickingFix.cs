using Exiled.Events.EventArgs;
using Exiled.Events.Handlers;
using GameCore;
using HarmonyLib;
using Mirror;
using System;
using UnityEngine;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), new[] { typeof(GameObject), typeof(int), typeof(string), typeof(string), typeof(bool) })]
    internal class BanningAndKickingFix
    {
        private static bool Prefix(GameObject user, int duration, string reason, string issuer, bool isGlobalBan)
        {
            try
            {

                Exiled.API.Features.Player issuerPlayer = Exiled.API.Features.Player.Get(issuer) ?? Exiled.API.Features.Server.Host;

                if (user.GetComponent<NPCComponent>() != null)
                {
                    issuerPlayer.RemoteAdminMessage("WTF U TRIED TO BAN NPC?", false, Plugin.Instance.Name);
                    issuerPlayer.ClearBroadcasts();
                    issuerPlayer.Broadcast(5,"<color=red>DONT BAN OR KICK NPCs</color>");
                    return false;
                }

                if (isGlobalBan && ConfigFile.ServerConfig.GetBool("gban_ban_ip", false))
                {
                    duration = int.MaxValue;
                }

                string userId = null;
                string address = user.GetComponent<NetworkIdentity>().connectionToClient.address;

                Exiled.API.Features.Player targetPlayer = Exiled.API.Features.Player.Get(user);

                try
                {
                    if (ConfigFile.ServerConfig.GetBool("online_mode", false))
                        userId = targetPlayer.UserId;
                }
                catch
                {
                    ServerConsole.AddLog("Failed during issue of User ID ban (1)!");
                    return false;
                }

                string message = $"You have been {((duration > 0) ? "banned" : "kicked")}. ";
                if (!string.IsNullOrEmpty(reason))
                {
                    message = message + "Reason: " + reason;
                }

                if (!ServerStatic.PermissionsHandler.IsVerified || !targetPlayer.IsStaffBypassEnabled)
                {
                    if (duration > 0)
                    {
                        var ev = new BanningEventArgs(targetPlayer, issuerPlayer, duration, reason, message);

                        Player.OnBanning(ev);

                        duration = ev.Duration;
                        reason = ev.Reason;
                        message = ev.FullMessage;

                        if (!ev.IsAllowed)
                            return false;

                        string originalName = string.IsNullOrEmpty(targetPlayer.Nickname)
                            ? "(no nick)"
                            : targetPlayer.Nickname;
                        long issuanceTime = TimeBehaviour.CurrentTimestamp();
                        long banExpieryTime = TimeBehaviour.GetBanExpieryTime((uint)duration);
                        try
                        {
                            if (userId != null && !isGlobalBan)
                            {
                                BanHandler.IssueBan(
                                    new BanDetails
                                    {
                                        OriginalName = originalName,
                                        Id = userId,
                                        IssuanceTime = issuanceTime,
                                        Expires = banExpieryTime,
                                        Reason = reason,
                                        Issuer = issuer,
                                    }, BanHandler.BanType.UserId);

                                if (!string.IsNullOrEmpty(targetPlayer.CustomUserId))
                                {
                                    BanHandler.IssueBan(
                                        new BanDetails
                                        {
                                            OriginalName = originalName,
                                            Id = targetPlayer.CustomUserId,
                                            IssuanceTime = issuanceTime,
                                            Expires = banExpieryTime,
                                            Reason = reason,
                                            Issuer = issuer,
                                        }, BanHandler.BanType.UserId);
                                }
                            }
                        }
                        catch
                        {
                            ServerConsole.AddLog("Failed during issue of User ID ban (2)!");
                            return false;
                        }

                        try
                        {
                            if (ConfigFile.ServerConfig.GetBool("ip_banning", false) || isGlobalBan)
                            {
                                BanHandler.IssueBan(
                                    new BanDetails
                                    {
                                        OriginalName = originalName,
                                        Id = address,
                                        IssuanceTime = issuanceTime,
                                        Expires = banExpieryTime,
                                        Reason = reason,
                                        Issuer = issuer,
                                    }, BanHandler.BanType.IP);
                            }
                        }
                        catch
                        {
                            ServerConsole.AddLog("Failed during issue of IP ban!");
                            return false;
                        }
                    }
                    else if (duration == 0)
                    {
                        var ev = new KickingEventArgs(targetPlayer, issuerPlayer, reason, message);

                        Player.OnKicking(ev);

                        reason = ev.Reason;
                        message = ev.FullMessage;

                        if (!ev.IsAllowed)
                            return false;
                    }
                }

                ServerConsole.Disconnect(targetPlayer.ReferenceHub.gameObject, message);

                return false;
            }
            catch (Exception e)
            {
                Exiled.API.Features.Log.Error($"Exiled.Events.Patches.Events.Player.BanningAndKicking: {e}\n{e.StackTrace}");

                return true;
            }
        }
    }
}