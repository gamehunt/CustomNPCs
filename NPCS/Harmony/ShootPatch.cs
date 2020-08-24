using Exiled.API.Features;
using HarmonyLib;
using System;
using UnityEngine;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.CallCmdShoot))]
    [HarmonyPriority(Priority.First)]
    internal class ShootPatch
    {
        private static bool Prefix(WeaponManager __instance, GameObject target, string hitboxType, Vector3 dir, Vector3 sourcePos, Vector3 targetPos)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true))
                    return false;
                bool is_npc = __instance.gameObject.GetComponent<Npc>() != null;
                int itemIndex = __instance._hub.inventory.GetItemIndex();
                if (!is_npc)
                {
                    if (itemIndex < 0 || itemIndex >= __instance._hub.inventory.items.Count || __instance.curWeapon < 0 ||
                        ((__instance._reloadCooldown > 0.0 || __instance._fireCooldown > 0.0) &&
                         !__instance.isLocalPlayer) ||
                        (__instance._hub.inventory.curItem != __instance.weapons[__instance.curWeapon].inventoryID ||
                         __instance._hub.inventory.items[itemIndex].durability <= 0.0))
                        return false;
                }

                Log.Debug("Invoking shooting event", Exiled.Loader.Loader.ShouldDebugBeShown);

                var shootingEventArgs = new Exiled.Events.EventArgs.ShootingEventArgs(Player.Get(__instance.gameObject), target, targetPos);

                Exiled.Events.Handlers.Player.OnShooting(shootingEventArgs);

                if (!shootingEventArgs.IsAllowed)
                    return false;

                targetPos = shootingEventArgs.Position;

                if (Vector3.Distance(__instance.camera.transform.position, sourcePos) > 6.5)
                {
                    __instance.GetComponent<CharacterClassManager>().TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code 2.2 (difference between real source position and provided source position is too big)", "gray");
                }
                else
                {
                    if (!is_npc)
                    {
                        __instance._hub.inventory.items.ModifyDuration(itemIndex, __instance._hub.inventory.items[itemIndex].durability - 1f);
                    }
                    __instance.scp268.ServerDisable();
                    __instance._fireCooldown = (float)(1.0 / (__instance.weapons[__instance.curWeapon].shotsPerSecond * (double)__instance.weapons[__instance.curWeapon].allEffects.firerateMultiplier) * 0.800000011920929);
                    float sourceRangeScale = __instance.weapons[__instance.curWeapon].allEffects.audioSourceRangeScale;
                    __instance.GetComponent<Scp939_VisionController>().MakeNoise(
                        Mathf.Clamp((float)(sourceRangeScale * (double)sourceRangeScale * 70.0), 5f, 100f));
                    bool flag = target != null;
                    if (targetPos == Vector3.zero)
                    {
                        if (Physics.Raycast(sourcePos, dir, out RaycastHit raycastHit, 500f, __instance.raycastMask))
                        {
                            HitboxIdentity component = raycastHit.collider.GetComponent<HitboxIdentity>();
                            if (component != null)
                            {
                                WeaponManager componentInParent = component.GetComponentInParent<WeaponManager>();
                                if (componentInParent != null)
                                {
                                    flag = false;
                                    target = componentInParent.gameObject;
                                    hitboxType = component.id;
                                    targetPos = componentInParent.transform.position;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Physics.Linecast(sourcePos, targetPos, out RaycastHit raycastHit, __instance.raycastMask))
                        {
                            HitboxIdentity component = raycastHit.collider.GetComponent<HitboxIdentity>();
                            if (component != null)
                            {
                                WeaponManager componentInParent = component.GetComponentInParent<WeaponManager>();
                                if (componentInParent != null)
                                {
                                    if (componentInParent.gameObject == target)
                                    {
                                        flag = false;
                                    }
                                    else if (componentInParent.scp268.Enabled)
                                    {
                                        flag = false;
                                        target = componentInParent.gameObject;
                                        hitboxType = component.id;
                                        targetPos = componentInParent.transform.position;
                                    }
                                }
                            }
                        }
                    }

                    CharacterClassManager c = null;

                    if (target != null)
                        c = target.GetComponent<CharacterClassManager>();

                    if (c != null && __instance.GetShootPermission(c, false))
                    {
                        if (Math.Abs(__instance.camera.transform.position.y - c.transform.position.y) > 40.0)
                        {
                            if (!is_npc)
                            {
                                __instance.GetComponent<CharacterClassManager>().TargetConsolePrint(
                                    __instance.connectionToClient,
                                    "Shot rejected - Code 2.1 (too big Y-axis difference between source and target)",
                                    "gray");
                            }
                            else
                            {
                                Log.Debug("NPC Shot rejected - Code 2.1 (too big Y-axis difference between source and target)", Plugin.Instance.Config.VerboseOutput);
                            }
                        }
                        else if (Vector3.Distance(c.transform.position, targetPos) > 6.5)
                        {
                            if (!is_npc)
                            {
                                __instance.GetComponent<CharacterClassManager>().TargetConsolePrint(
                                __instance.connectionToClient,
                                "Shot rejected - Code 2.3 (difference between real target position and provided target position is too big)",
                                "gray");
                            }
                            else
                            {
                                Log.Debug("NPC Shot rejected - Code 2.3 (difference between real target position and provided target position is too big)", Plugin.Instance.Config.VerboseOutput);
                            }
                        }
                        else if (Physics.Linecast(__instance.camera.transform.position, sourcePos, __instance.raycastServerMask))
                        {
                            if (!is_npc)
                            {
                                __instance.GetComponent<CharacterClassManager>().TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code 2.4 (collision between source positions detected)", "gray");
                            }
                            else
                            {
                                Log.Debug("NPC Shot rejected - Code 2.4 (collision between source positions detected)", Plugin.Instance.Config.VerboseOutput);
                            }
                        }
                        else if (flag && Physics.Linecast(sourcePos, targetPos, __instance.raycastServerMask))
                        {
                            if (!is_npc)
                            {
                                __instance.GetComponent<CharacterClassManager>().TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code 2.5 (collision on shot line detected)", "gray");
                            }
                            else
                            {
                                Log.Debug("NPC Shot rejected - Code 2.5 (collision on shot line detected)", Plugin.Instance.Config.VerboseOutput);
                            }
                        }
                        else
                        {
                            float num1 = Vector3.Distance(__instance.camera.transform.position, target.transform.position);
                            float num2 = __instance.weapons[__instance.curWeapon].damageOverDistance.Evaluate(num1);
                            string upper = hitboxType.ToUpper();
                            if (upper != "HEAD")
                            {
                                if (upper != "LEG")
                                {
                                    if (upper == "SCP106")
                                        num2 /= 10f;
                                }
                                else
                                {
                                    num2 /= 2f;
                                }
                            }
                            else
                            {
                                num2 *= 4f;
                            }

                            Log.Debug("Invoking late shoot.", Exiled.Loader.Loader.ShouldDebugBeShown);

                            var shotEventArgs = new Exiled.Events.EventArgs.ShotEventArgs(Player.Get(__instance.gameObject), target, hitboxType, num1, num2);

                            Exiled.Events.Handlers.Player.OnShot(shotEventArgs);

                            if (!shotEventArgs.CanHurt)
                                return false;

                            __instance._hub.playerStats.HurtPlayer(
                                new PlayerStats.HitInfo(
                                    shotEventArgs.Damage * __instance.weapons[__instance.curWeapon].allEffects.damageMultiplier *
                                    __instance.overallDamagerFactor,
                                    __instance._hub.nicknameSync.MyNick + " (" + __instance._hub.characterClassManager.UserId + ")",
                                    DamageTypes.FromWeaponId(__instance.curWeapon),
                                    __instance._hub.queryProcessor.PlayerId), c.gameObject);
                            __instance.RpcConfirmShot(true, __instance.curWeapon);
                            __instance.PlaceDecal(true, new Ray(__instance.camera.position, dir), (int)c.CurClass, shotEventArgs.Damage);
                        }
                    }
                    else if (target != null && hitboxType == "window" && target.GetComponent<BreakableWindow>() != null)
                    {
                        float damage = __instance.weapons[__instance.curWeapon].damageOverDistance
                            .Evaluate(Vector3.Distance(__instance.camera.transform.position, target.transform.position));
                        target.GetComponent<BreakableWindow>().ServerDamageWindow(damage);
                        __instance.RpcConfirmShot(true, __instance.curWeapon);
                    }
                    else
                    {
                        __instance.PlaceDecal(false, new Ray(__instance.camera.position, dir), __instance.curWeapon, 0.0f);
                        __instance.RpcConfirmShot(false, __instance.curWeapon);
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Exiled.API.Features.Log.Error($"Exiled.Events.Patches.Events.Player.Shoot (Repatched by CustomNPCs!!!): {e}\n{e.StackTrace}");

                return true;
            }
        }
    }
}