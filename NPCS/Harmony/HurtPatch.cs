using HarmonyLib;
using Mirror;
using UnityEngine;

//TODO fix
namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.HurtPlayer))]
    [HarmonyPriority(Priority.Last)]
    internal class HurtPatch
    {
		private static bool Prefix(PlayerStats __instance, ref bool __result, global::PlayerStats.HitInfo info, GameObject go, bool noTeamDamage = false)
		{
			bool is_npc = __instance.GetComponent<Npc>() != null;
			bool is_shooter_npc = go.GetComponent<Npc>() != null;
			if (!is_npc && !is_shooter_npc)
			{
				return true;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = go == null;
			global::ReferenceHub referenceHub = flag3 ? null : global::ReferenceHub.GetHub(go);
			if (info.Amount < 0f)
			{
				if (flag3)
				{
					info.Amount = Mathf.Abs(999999f);
				}
				else
				{
					info.Amount = ((referenceHub.playerStats != null) ? Mathf.Abs(referenceHub.playerStats.Health + referenceHub.playerStats.syncArtificialHealth + 10f) : Mathf.Abs(999999f));
				}
			}
			if (__instance._burned.Enabled)
			{
				info.Amount *= __instance._burned.DamageMult;
			}
			if (info.Amount > 2.14748365E+09f)
			{
				info.Amount = 2.14748365E+09f;
			}
			if (info.GetDamageType().isWeapon && referenceHub.characterClassManager.IsAnyScp() && info.GetDamageType() != global::DamageTypes.MicroHid)
			{
				info.Amount *= __instance.weaponManager.weapons[(int)__instance.weaponManager.curWeapon].scpDamageMultiplier;
			}
			if (flag3)
			{
				__result = false;
				return false;
			}
			global::PlayerStats playerStats = referenceHub.playerStats;
			global::CharacterClassManager characterClassManager = referenceHub.characterClassManager;
			if (playerStats == null || characterClassManager == null)
			{
				__result = false;
				return false;
			}
			if (characterClassManager.GodMode)
			{
				__result = false;
				return false;
			}
			if (__instance.ccm.CurRole.team == global::Team.SCP && __instance.ccm.Classes.SafeGet(characterClassManager.CurClass).team == global::Team.SCP && __instance.ccm != characterClassManager)
			{
				__result = false;
				return false;
			}
			if (characterClassManager.SpawnProtected && !__instance._allowSPDmg)
			{
				__result = false;
				return false;
			}
			bool flag4 = !noTeamDamage && info.IsPlayer && referenceHub != info.RHub && referenceHub.characterClassManager.Fraction == info.RHub.characterClassManager.Fraction;
			if (flag4)
			{
				info.Amount *= global::PlayerStats.FriendlyFireFactor;
			}
			float health = playerStats.Health;
			if (__instance.lastHitInfo.Attacker == "ARTIFICIALDEGEN")
			{
				playerStats.unsyncedArtificialHealth -= info.Amount;
				if (playerStats.unsyncedArtificialHealth < 0f)
				{
					playerStats.unsyncedArtificialHealth = 0f;
				}
			}
			else
			{
				if (playerStats.unsyncedArtificialHealth > 0f)
				{
					float num = info.Amount * playerStats.artificialNormalRatio;
					float num2 = info.Amount - num;
					playerStats.unsyncedArtificialHealth -= num;
					if (playerStats.unsyncedArtificialHealth < 0f)
					{
						num2 += Mathf.Abs(playerStats.unsyncedArtificialHealth);
						playerStats.unsyncedArtificialHealth = 0f;
					}
					playerStats.Health -= num2;
					if (playerStats.Health > 0f && playerStats.Health - num <= 0f && characterClassManager.CurRole.team != global::Team.SCP && !is_npc)
					{
						__instance.TargetAchieve(characterClassManager.connectionToClient, "didntevenfeelthat");
					}
				}
				else
				{
					playerStats.Health -= info.Amount;
				}
				if (playerStats.Health < 0f)
				{
					playerStats.Health = 0f;
				}
				playerStats.lastHitInfo = info;
			}
			global::PlayableScpsController component = go.GetComponent<global::PlayableScpsController>();
			PlayableScps.Interfaces.IDamagable damagable;
			if (component != null && (damagable = (component.CurrentScp as PlayableScps.Interfaces.IDamagable)) != null)
			{
				damagable.OnDamage(info);
			}
			if (playerStats.Health < 1f && characterClassManager.CurClass != global::RoleType.Spectator)
			{
				PlayableScps.Interfaces.IImmortalScp immortalScp;
				if (component != null && (immortalScp = (component.CurrentScp as PlayableScps.Interfaces.IImmortalScp)) != null && !immortalScp.OnDeath(info, __instance.gameObject))
				{
					__result = false;
					return false;
				}
				foreach (global::Scp079PlayerScript scp079PlayerScript in global::Scp079PlayerScript.instances)
				{
					global::Scp079Interactable.ZoneAndRoom otherRoom = go.GetComponent<global::Scp079PlayerScript>().GetOtherRoom();
					bool flag5 = false;
					foreach (global::Scp079Interaction scp079Interaction in scp079PlayerScript.ReturnRecentHistory(12f, __instance._filters))
					{
						foreach (global::Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interaction.interactable.currentZonesAndRooms)
						{
							if (zoneAndRoom.currentZone == otherRoom.currentZone && zoneAndRoom.currentRoom == otherRoom.currentRoom)
							{
								flag5 = true;
							}
						}
					}
					if (flag5)
					{
						scp079PlayerScript.RpcGainExp(global::ExpGainType.KillAssist, characterClassManager.CurClass);
					}
				}
				if (global::RoundSummary.RoundInProgress() && global::RoundSummary.roundTime < 60 && !is_npc)
				{
					__instance.TargetAchieve(characterClassManager.connectionToClient, "wowreally");
				}
				if (__instance.isLocalPlayer && info.PlayerId != referenceHub.queryProcessor.PlayerId)
				{
					global::RoundSummary.Kills++;
				}
				flag = true;
				if (!is_npc)
				{
					if (characterClassManager.CurClass == global::RoleType.Scp096)
					{
						global::ReferenceHub hub = global::ReferenceHub.GetHub(go);
						if (hub != null && hub.scpsController.CurrentScp is PlayableScps.Scp096 && (hub.scpsController.CurrentScp as PlayableScps.Scp096).PlayerState == PlayableScps.Scp096PlayerState.Enraging)
						{
							__instance.TargetAchieve(characterClassManager.connectionToClient, "unvoluntaryragequit");
						}
					}
					else if (info.GetDamageType() == global::DamageTypes.Pocket)
					{
						__instance.TargetAchieve(characterClassManager.connectionToClient, "newb");
					}
					else if (info.GetDamageType() == global::DamageTypes.Scp173)
					{
						__instance.TargetAchieve(characterClassManager.connectionToClient, "firsttime");
					}
					else if (info.GetDamageType() == global::DamageTypes.Grenade && info.PlayerId == referenceHub.queryProcessor.PlayerId)
					{
						__instance.TargetAchieve(characterClassManager.connectionToClient, "iwanttobearocket");
					}
					else if (info.GetDamageType().isWeapon)
					{
						global::Inventory inventory = referenceHub.inventory;
						if (characterClassManager.CurClass == global::RoleType.Scientist)
						{
							global::Item itemByID = inventory.GetItemByID(inventory.curItem);
							if (itemByID != null && itemByID.itemCategory == global::ItemCategory.Keycard && __instance.GetComponent<global::CharacterClassManager>().CurClass == global::RoleType.ClassD)
							{
								__instance.TargetAchieve(__instance.connectionToClient, "betrayal");
							}
						}
						if (Time.realtimeSinceStartup - __instance._killStreakTime > 30f || __instance._killStreak == 0)
						{
							__instance._killStreak = 0;
							__instance._killStreakTime = Time.realtimeSinceStartup;
						}
						if (__instance.GetComponent<global::WeaponManager>().GetShootPermission(characterClassManager, true))
						{
							__instance._killStreak++;
						}
						if (__instance._killStreak > 5)
						{
							__instance.TargetAchieve(__instance.connectionToClient, "pewpew");
						}
						if ((__instance.ccm.CurRole.team == global::Team.MTF || __instance.ccm.Classes.SafeGet(__instance.ccm.CurClass).team == global::Team.RSC) && characterClassManager.CurClass == global::RoleType.ClassD)
						{
							__instance.TargetStats(__instance.connectionToClient, "dboys_killed", "justresources", 50);
						}
					}
					else if (__instance.ccm.CurRole.team == global::Team.SCP && go.GetComponent<global::MicroHID>().CurrentHidState != global::MicroHID.MicroHidState.Idle)
					{
						__instance.TargetAchieve(__instance.connectionToClient, "illpassthanks");
					}
					if (__instance.ccm.CurRole.team == global::Team.RSC && __instance.ccm.Classes.SafeGet(characterClassManager.CurClass).team == global::Team.SCP)
					{
						__instance.TargetAchieve(__instance.connectionToClient, "timetodoitmyself");
					}
				}
				bool flag6 = info.IsPlayer && referenceHub == info.RHub;
				flag2 = flag4;
				if (flag6)
				{
					global::ServerLogs.AddLog(global::ServerLogs.Modules.ClassChange, string.Concat(new string[]
					{
					referenceHub.LoggedNameFromRefHub(),
					" playing as ",
					referenceHub.characterClassManager.CurRole.fullName,
					" committed a suicide using ",
					info.GetDamageName(),
					"."
					}), global::ServerLogs.ServerLogType.Suicide, false);
				}
				else
				{
					global::ServerLogs.AddLog(global::ServerLogs.Modules.ClassChange, string.Concat(new string[]
					{
					referenceHub.LoggedNameFromRefHub(),
					" playing as ",
					referenceHub.characterClassManager.CurRole.fullName,
					" has been killed by ",
					info.Attacker,
					" using ",
					info.GetDamageName(),
					info.IsPlayer ? (" playing as " + info.RHub.characterClassManager.CurRole.fullName + ".") : "."
					}), flag2 ? global::ServerLogs.ServerLogType.Teamkill : global::ServerLogs.ServerLogType.KillLog, false);
				}
				if (info.GetDamageType().isScp || info.GetDamageType() == global::DamageTypes.Pocket)
				{
					global::RoundSummary.kills_by_scp++;
				}
				else if (info.GetDamageType() == global::DamageTypes.Grenade)
				{
					global::RoundSummary.kills_by_frag++;
				}
				if (!__instance._pocketCleanup || info.GetDamageType() != global::DamageTypes.Pocket)
				{
					referenceHub.inventory.ServerDropAll();
					global::PlayerMovementSync playerMovementSync = referenceHub.playerMovementSync;
					if (characterClassManager.Classes.CheckBounds(characterClassManager.CurClass) && info.GetDamageType() != global::DamageTypes.RagdollLess)
					{
						__instance.GetComponent<global::RagdollManager>().SpawnRagdoll(go.transform.position, go.transform.rotation, (playerMovementSync == null) ? Vector3.zero : playerMovementSync.PlayerVelocity, (int)characterClassManager.CurClass, info, characterClassManager.CurRole.team > global::Team.SCP, go.GetComponent<Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer>().PlayerId, referenceHub.nicknameSync.DisplayName, referenceHub.queryProcessor.PlayerId);
					}
				}
				else
				{
					referenceHub.inventory.Clear();
				}
				characterClassManager.NetworkDeathPosition = go.transform.position;
				if (characterClassManager.CurRole.team == global::Team.SCP)
				{
					if (characterClassManager.CurClass == global::RoleType.Scp0492)
					{
						global::NineTailedFoxAnnouncer.CheckForZombies(go);
					}
					else
					{
						GameObject x = null;
						foreach (GameObject gameObject in global::PlayerManager.players)
						{
							if (gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId == info.PlayerId)
							{
								x = gameObject;
							}
						}
						if (x != null)
						{
							global::NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, string.Empty);
						}
						else
						{
							global::DamageTypes.DamageType damageType = info.GetDamageType();
							if (damageType == global::DamageTypes.Tesla)
							{
								global::NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "TESLA");
							}
							else if (damageType == global::DamageTypes.Nuke)
							{
								global::NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "WARHEAD");
							}
							else if (damageType == global::DamageTypes.Decont)
							{
								global::NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "DECONTAMINATION");
							}
							else if (characterClassManager.CurClass != global::RoleType.Scp079)
							{
								global::NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "UNKNOWN");
							}
						}
					}
				}
				playerStats.SetHPAmount(100);
				characterClassManager.SetClassID(global::RoleType.Spectator);
			}
			else
			{
				Vector3 pos = Vector3.zero;
				float num3 = 40f;
				if (info.GetDamageType().isWeapon)
				{
					GameObject playerOfID = __instance.GetPlayerOfID(info.PlayerId);
					if (playerOfID != null)
					{
						pos = go.transform.InverseTransformPoint(playerOfID.transform.position).normalized;
						num3 = 100f;
					}
				}
				else if (info.GetDamageType() == global::DamageTypes.Pocket)
				{
					global::PlayerMovementSync component2 = __instance.ccm.GetComponent<global::PlayerMovementSync>();
					if (component2.RealModelPosition.y > -1900f)
					{
						component2.OverridePosition(Vector3.down * 1998.5f, 0f, true);
					}
				}
				if (!is_shooter_npc)
				{
					__instance.TargetBloodEffect(go.GetComponent<NetworkIdentity>().connectionToClient, pos, Mathf.Clamp01(info.Amount / num3));
				}
			}
			Respawning.RespawnTickets singleton = Respawning.RespawnTickets.Singleton;
			global::Team team = characterClassManager.CurRole.team;
			byte b = (byte)team;
			if (b != 0)
			{
				if (b == 3)
				{
					if (flag)
					{
						global::Team team2 = __instance.ccm.Classes.SafeGet(characterClassManager.CurClass).team;
						if (team2 == global::Team.CDP && team2 == global::Team.CHI)
						{
							singleton.GrantTickets(Respawning.SpawnableTeamType.ChaosInsurgency, __instance._respawn_tickets_ci_scientist_died_count, false);
						}
					}
				}
			}
			else if (characterClassManager.CurClass != global::RoleType.Scp0492)
			{
				for (float num4 = 1f; num4 > 0f; num4 -= __instance._respawn_tickets_mtf_scp_hurt_interval)
				{
					float num5 = (float)playerStats.maxHP * num4;
					if (health > num5 && playerStats.Health < num5)
					{
						singleton.GrantTickets(Respawning.SpawnableTeamType.NineTailedFox, __instance._respawn_tickets_mtf_scp_hurt_count, false);
					}
				}
			}
			PlayableScps.Interfaces.IDamagable damagable2;
			if (component != null && (damagable2 = (component.CurrentScp as PlayableScps.Interfaces.IDamagable)) != null)
			{
				damagable2.OnDamage(info);
			}
			if (!flag4 || FriendlyFireConfig.PauseDetector || global::PermissionsHandler.IsPermitted(info.RHub.serverRoles.Permissions, global::PlayerPermissions.FriendlyFireDetectorImmunity))
			{
				__result = flag;
				return false;
			}
			if (FriendlyFireConfig.IgnoreClassDTeamkills && referenceHub.characterClassManager.CurRole.team == global::Team.CDP && info.RHub.characterClassManager.CurRole.team == global::Team.CDP)
			{
				__result = flag;
				return false;
			}
			if (!is_npc)
			{
				if (flag2)
				{
					if (info.RHub.FriendlyFireHandler.Respawn.RegisterKill())
					{
						__result = flag;
						return false;
					}
					if (info.RHub.FriendlyFireHandler.Window.RegisterKill())
					{
						__result = flag;
						return false;
					}
					if (info.RHub.FriendlyFireHandler.Life.RegisterKill())
					{
						__result = flag;
						return false;
					}
					if (info.RHub.FriendlyFireHandler.Round.RegisterKill())
					{
						__result = flag;
						return false;
					}
				}
				if (info.RHub.FriendlyFireHandler.Respawn.RegisterDamage(info.Amount))
				{
					__result = flag;
					return false;
				}
				if (info.RHub.FriendlyFireHandler.Window.RegisterDamage(info.Amount))
				{
					__result = flag;
					return false;
				}
				if (info.RHub.FriendlyFireHandler.Life.RegisterDamage(info.Amount))
				{
					__result = flag;
					return false;
				}
				info.RHub.FriendlyFireHandler.Round.RegisterDamage(info.Amount);
			}
			__result = flag;
			return false;
		}
    }
}