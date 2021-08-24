using System;
using System.Collections.Generic;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items.Armor;
using Synapse.Api;
using Synapse.Api.Enum;
using Synapse.Api.Events.SynapseEventArguments;
using Synapse.Api.Items;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetPlayersClass))]
    internal static class SetPlayersClassPatch
    {
        [HarmonyPrefix]
        private static bool OnSetClass(ref RoleType classid, GameObject ply, CharacterClassManager.SpawnReason spawnReason, bool lite = false)
        {
            try
            {
                var player = ply.GetPlayer();

                if (player.Hub.isDedicatedServer || !player.Hub.Ready) return false;

                //Initialise eventargs
                var eventargs = new PlayerSetClassEventArgs
                {
                    Allow = true,
                    Player = player,
                    Role = classid,
                    SpawnReason = spawnReason,
                    EscapeItems = new List<SynapseItem>(),
                    Position = Vector3.zero,
                    Rotation = 0f,
                    Items = new List<SynapseItem>(),
                    Ammo = new Dictionary<AmmoType, ushort>(),
                };

                //Set EscapeItems if the Player is escaping
                if (eventargs.IsEscaping) eventargs.EscapeItems = player.Inventory.Items;

                //Find the Position and Rotation if the player becomes a living Role
                if(classid != RoleType.Spectator && classid != RoleType.None)
                {
                    var randomPosition = CharacterClassManager._spawnpointManager.GetRandomPosition(classid);
                    if (Map.Get.RespawnPoint != Vector3.zero)
                    {
                        eventargs.Position = Map.Get.RespawnPoint;
                    }
                    else if (randomPosition != null)
                    {
                        eventargs.Position = randomPosition.transform.position;
                        eventargs.Rotation = randomPosition.transform.rotation.eulerAngles.y;
                    }
                    else
                    {
                        eventargs.Position = player.ClassManager.DeathPosition;
                    }
                }

                //Find and create the Items that the Player should spawn with
                if(InventorySystem.Configs.StartingInventories.DefinedInventories.TryGetValue(classid,out var roleitems))
                {
                    foreach (var ammo in roleitems.Ammo)
                        eventargs.Ammo[(AmmoType)ammo.Key] = ammo.Value;

                    foreach (var itemtype in roleitems.Items)
                        eventargs.Items.Add(new SynapseItem(itemtype));
                }

                Server.Get.Events.Player.InvokeSetClassEvent(eventargs);

                classid = eventargs.Role;

                if (eventargs.Allow)
                    player.setClassEventArgs = eventargs;

                return eventargs.Allow;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerSetClass(SetPlayersClass) failed!!\n{e}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(InventoryItemProvider),nameof(InventoryItemProvider.RoleChanged))]
    internal static class HandleItemPatch
    {
        [HarmonyPrefix]
        private static bool OnRoleChanged(ReferenceHub ply, RoleType prevRole, RoleType newRole, bool lite, CharacterClassManager.SpawnReason spawnReason)
        {
            try
            {
                var player = ply.GetPlayer();
                var args = player.setClassEventArgs;

                //If args is null he is SCP0492 and should not get any Items
                if (args == null) return false;

                var inventory = ply.inventory;

                if(args.IsEscaping)
                {
                    foreach (var item in player.Inventory.Items)
                        item.Despawn();
                }
                else player.Inventory.Clear();

                foreach (var ammo in args.Ammo)
                    inventory.ServerAddAmmo((ItemType)ammo.Key, ammo.Value);

                foreach (var item in args.Items)
                {
                    item.Drop();
                    var arg = player.VanillaInventory.ServerAddItem(item.ItemType, item.Serial, item.PickupBase);

                    var onItemProvided = InventoryItemProvider.OnItemProvided;

                    if (onItemProvided != null)
                    {
                        onItemProvided(player.Hub, arg);
                    }
                }

                if (args.IsEscaping)
                {
                    foreach(var item in args.EscapeItems)
                    {
                        if(inventory.UserInventory.Items.Count < 8 && item.ItemCategory != ItemCategory.Armor)
                        {
                            item.PickUp(player);
                            InventorySystem.Items.Armor.BodyArmorUtils.RemoveEverythingExceedingLimits(inventory, 
                                inventory.TryGetBodyArmor(out var bodyArmor) ? bodyArmor : null, true, true);
                        }
                        else item.Drop(args.Position);
                    }
                }

                args.CanBeDeleted = true;

                return false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerSetClass(Items) failed!!\n{e}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager),nameof(CharacterClassManager.ApplyProperties))]
    internal static class ApplyPropertiesPatch
    {
        [HarmonyPrefix]
        private static bool OnApplyProperties(CharacterClassManager __instance,bool lite)
        {
            try
            {
                if (lite) return true;

                var player = __instance.GetPlayer();

                var curRole = __instance.CurRole;
                if (!__instance._wasAnytimeAlive && __instance.CurClass != RoleType.Spectator && __instance.CurClass != RoleType.None)
                    __instance._wasAnytimeAlive = true;

                __instance.InitSCPs();
                __instance.AliveTime = 0f;

                var team = curRole.team;
                if (team - Team.RSC <= 1)
                    __instance.EscapeStartTime = (int)Time.realtimeSinceStartup;

                try
                {
                    __instance._hub.footstepSync.SetLoudness(curRole.team, curRole.roleId.Is939());
                }
                catch
                {
                }

                if (curRole.roleId != RoleType.Spectator 
                    && Respawning.RespawnManager.CurrentSequence() != Respawning.RespawnManager.RespawnSequencePhase.SpawningSelectedTeam 
                    && Respawning.NamingRules.UnitNamingManager.RolesWithEnforcedDefaultName.TryGetValue(curRole.roleId, out var spawnableTeamType) 
                    && Respawning.RespawnManager.Singleton.NamingManager.TryGetAllNamesFromGroup((byte)spawnableTeamType, out var array) 
                    && array.Length != 0)
                {
                    __instance.NetworkCurSpawnableTeamType = (byte)spawnableTeamType;
                    __instance.NetworkCurUnitName = array[0];
                }
                else if (__instance.CurSpawnableTeamType != 0)
                {
                    __instance.NetworkCurSpawnableTeamType = 0;
                    __instance.NetworkCurUnitName = string.Empty;
                }
                if (curRole.team != Team.RIP)
                {
                    if (!lite)
                    {
                        var args = player.setClassEventArgs;

                        //It is null when SCP-049 "cures" a human to SCP-049-2
                        if (args == null)
                            __instance._pms.OnPlayerClassChange(__instance.DeathPosition, 0f);
                        else
                        {
                            __instance._pms.OnPlayerClassChange(args.Position, args.Rotation);
                            __instance._pms.IsAFK = true;
                        }

                        if (!__instance.SpawnProtected && CharacterClassManager.EnableSP && CharacterClassManager.SProtectedTeam.Contains((int)curRole.team))
                        {
                            __instance.GodMode = true;
                            __instance.SpawnProtected = true;
                            __instance.ProtectedTime = Time.time;
                        }

                        if (args.CanBeDeleted) player.setClassEventArgs = null;
                    }
                    if (!__instance.isLocalPlayer)
                    {
                        __instance._hub.playerStats.maxHP = curRole.maxHP;
                    }
                }

                __instance.Scp0492.iAm049_2 = (__instance.CurClass == RoleType.Scp0492);
                __instance.Scp106.iAm106 = (__instance.CurClass == RoleType.Scp106);
                __instance.Scp939.iAm939 = __instance.CurClass.Is939();
                __instance.RefreshPlyModel(RoleType.None);

                return false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerSetClass(ApplyProperties) failed!!\n{e}");
                return true;
            }
        }
    }
}
