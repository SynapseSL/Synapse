using System;
using HarmonyLib;
using System.Collections.Generic;
using Logger = Synapse.Api.Logger;
using EventHandler = Synapse.Api.Events.EventHandler;
using UnityEngine;
using Synapse.Api;
using System.Linq;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdOpenDoor))]
    internal static class DoorInteractPatch
    {
        private static bool Prefix(PlayerInteract __instance, GameObject doorId)
        {
            try
            {
                var player = __instance.GetPlayer();
                var keycard = player.ItemInHand;
                if (keycard?.ItemCategory != ItemCategory.Keycard)
                    keycard = null;
                    

                var allowTheAccess = true;
                Door door = null;

                if (!__instance._playerInteractRateLimit.CanExecute() || (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract))
                    return false;

                if (doorId == null)
                    return false;

                if (player.RoleType == RoleType.None || player.RoleType == RoleType.Spectator)
                    return false;

                if (!doorId.TryGetComponent(out door))
                    return false;


                if ((door.Buttons.Count == 0) ? (!__instance.ChckDis(doorId.transform.position)) : Enumerable.All(door.Buttons, item => !__instance.ChckDis(item.button.transform.position)))
                    return false;

                __instance.OnInteract();

                if (!__instance._sr.BypassMode && !(door.PermissionLevels.HasPermission(Door.AccessRequirements.Checkpoints) &&
                         player.Team == Team.SCP))
                {
                    try
                    {
                        if (door.PermissionLevels == 0)
                            allowTheAccess = !door.locked;

                        else if (!door.RequireAllPermissions)
                        {
                            if (keycard == null)
                                allowTheAccess = false;
                            else
                            {
                                var allow = true;
                                try
                                {
                                    Server.Get.Events.Player.InvokePlayerItemUseEvent(player, keycard,
                                        Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allow);
                                }
                                catch(Exception e)
                                {
                                    Logger.Get.Error($"Synapse-Event: PlayerItemUseEvent(Keycard) failed!!\n{e}");
                                }

                                var itemPerms = __instance._inv.GetItemByID(__instance._inv.curItem).permissions;

                                allowTheAccess = allow && itemPerms.Any(p =>
                                    Door.backwardsCompatPermissions.TryGetValue(p, out var flag) &&
                                    door.PermissionLevels.HasPermission(flag));
                            }
                        }
                        else allowTheAccess = false;
                    }
                    catch
                    {
                        allowTheAccess = false;
                    }
                }

                var synapsedoor = Map.Get.Doors.FirstOrDefault(x => x.GameObject == doorId);

                if (Server.Get.Configs.SynapseConfiguration.RemoteKeyCard && !allowTheAccess && player.VanillaItems.Any())
                {
                    foreach(var item in player.Inventory.Items.Where(x => x.ItemCategory == ItemCategory.Keycard && x != keycard))
                    {
                        var gameItem = player.VanillaInventory.GetItemByID(item.ItemType);
                        var havepermission = false;

                        if (gameItem.permissions.Any(p =>
                             Door.backwardsCompatPermissions.TryGetValue(p, out var flag) &&
                             synapsedoor.PermissionLevels.HasPermission(flag)))
                            havepermission = true;

                        if (havepermission)
                        {
                            try
                            {
                                Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item,
                                            Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref havepermission);
                            }
                            catch (Exception e)
                            {
                                Logger.Get.Error($"Synapse-Event: PlayerItemUseEvent(Keycard-Remote) failed!!\n{e}");
                            }
                        }

                        if (havepermission)
                            allowTheAccess = true;
                    }
                }

                EventHandler.Get.Map.InvokeDoorInteractEvent(player, synapsedoor, ref allowTheAccess);

                if (allowTheAccess) door.ChangeState(__instance._sr.BypassMode);
                else __instance.RpcDenied(doorId);

                return false;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: DoorInteract failed!!\n{e}");
                return true;
            }
        }
    }
}
