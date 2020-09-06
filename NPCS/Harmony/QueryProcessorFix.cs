using HarmonyLib;
using Mirror;
using Org.BouncyCastle.Security;
using RemoteAdmin;
using System;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.Start))]
    internal class QueryProcessorFix
    {
        private static bool Prefix(QueryProcessor __instance)
        {
            __instance._commandRateLimit = __instance.GetComponent<Security.PlayerRateLimitHandler>().RateLimits[2];
            __instance._hub = global::ReferenceHub.GetHub(__instance.gameObject);
            __instance.Roles = __instance._hub.serverRoles;
            __instance.CryptoManager = __instance.GetComponent<RemoteAdminCryptographicManager>();
            __instance.GCT = __instance.GetComponent<global::GameConsoleTransmission>();
            if (QueryProcessor._secureRandom == null)
            {
                QueryProcessor._secureRandom = new SecureRandom();
            }
            __instance.SignaturesCounter = 0;
            __instance._signaturesCounter = 0;
            if (NetworkServer.active)
            {
                __instance.NetworkPlayerId = QueryProcessor._idIterator++;
                __instance._conns = __instance.connectionToClient;
                __instance._ipAddress = __instance._conns?.address;
                __instance.NetworkOverridePasswordEnabled = global::ServerStatic.PermissionsHandler.OverrideEnabled;
                if (string.IsNullOrEmpty(QueryProcessor._serverStaticRandom))
                {
                    byte[] array;
                    using (System.Security.Cryptography.RandomNumberGenerator randomNumberGenerator = new System.Security.Cryptography.RNGCryptoServiceProvider())
                    {
                        array = new byte[32];
                        randomNumberGenerator.GetBytes(array);
                    }
                    QueryProcessor._serverStaticRandom = Convert.ToBase64String(array);
                    global::ServerConsole.AddLog("Generated round random salt: " + QueryProcessor._serverStaticRandom, ConsoleColor.Gray);
                }
                if (string.IsNullOrEmpty(__instance.ServerRandom))
                {
                    __instance.NetworkServerRandom = QueryProcessor._serverStaticRandom;
                }
            }
            __instance._sender = new PlayerCommandSender(__instance);
            if (!__instance.isLocalPlayer)
            {
                return false;
            }
            QueryProcessor.Localplayer = __instance;
            QueryProcessor.LocalCCM = __instance.GetComponent<global::CharacterClassManager>();
            return false;
        }
    }
}