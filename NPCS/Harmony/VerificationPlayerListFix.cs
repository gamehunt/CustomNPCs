using System;
using System.Collections.Generic;
using System.Text;

using HarmonyLib;
using UnityEngine;

//Im not sure if this patch needed, but it's better to have it here
namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.FixedUpdate))]
    class VerificationPlayerListFix
    {
        private static void Prefix(ServerConsole __instance)
        {
			string text;
			while (global::ServerConsole.PrompterQueue.TryDequeue(out text))
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					ConsoleColor color;
					global::ServerConsole.AddLog(global::ServerConsole.EnterCommand(text, out color, null), color);
				}
			}
			if (global::ServerConsole._playersListRefresh < 5f)
			{
				global::ServerConsole._playersListRefresh += Time.fixedDeltaTime;
				return;
			}
			if (!global::ServerConsole.PlayersListChanged)
			{
				return;
			}
			global::ServerConsole.PlayersListChanged = false;
			global::ServerConsole._playersListRefresh = 0f;
			try
			{
				foreach (GameObject gameObject in global::PlayerManager.players)
				{
					if (!(gameObject == null))
					{
						global::CharacterClassManager component = gameObject.GetComponent<global::CharacterClassManager>();
						if (!(component == null) && component.IsVerified && component.GetComponent<Npc>() == null && !string.IsNullOrEmpty(component.UserId) && (!component.isLocalPlayer || !global::ServerStatic.IsDedicated))
						{
							global::ServerConsole.PlayersListRaw.objects.Add(component.UserId);
						}
					}
				}
				global::ServerConsole._verificationPlayersList = global::JsonSerialize.ToJson<global::PlayerListSerialized>(global::ServerConsole.PlayersListRaw);
				global::ServerConsole.PlayersListRaw.objects.Clear();
			}
			catch (Exception ex)
			{
				global::ServerConsole.AddLog("[VERIFICATION] Exception in Players Online processing: " + ex.Message, ConsoleColor.Gray);
				global::ServerConsole.AddLog(ex.StackTrace, ConsoleColor.Gray);
			}
		}
    }
}
