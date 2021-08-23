using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Configs;
using InventorySystem.Items.Armor;
using Respawning;
using Respawning.NamingRules;
using Synapse.Api;
using Synapse.Api.Enum;
using Synapse.Api.Events.SynapseEventArguments;
using Synapse.Api.Items;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetPlayersClass))]
    internal static class SetClassFirstpatch
    {
        [HarmonyPrefix]
        private static bool FirstPatch(CharacterClassManager __instance, CharacterClassManager.SpawnReason spawnReason, ref RoleType classid, GameObject ply, bool lite = false)
        {
            try
            {
                var player = ply.GetPlayer();

                var eventargs = new PlayerSetClassEventArgs
                {
                    EscapeItems = new List<SynapseItem>(),
                    IsEscaping = spawnReason == CharacterClassManager.SpawnReason.Escaped,
                    SpawnReason = spawnReason,
                    Allow = true,
                    Player = player,
                    Role = classid,
                    Items = new List<SynapseItem>(),
                    Ammo = new Dictionary<AmmoType, ushort>(),
                    Position = Vector3.zero,
                    Rotation = 0f
                };

                //Normally all of these Things are set later in the code but since we need it know already for the event we get them earlier and store them in the Player for the other Patches
                StartingInventories.DefinedInventories.TryGetValue(classid, out var roleinfo);
                eventargs.Items = roleinfo.Items?.ToList().Select(x => new SynapseItem(x)).ToList();
                eventargs.Ammo = (Dictionary<AmmoType, ushort>)roleinfo.Ammo?.Select(x => new KeyValuePair<AmmoType, ushort>((AmmoType)x.Key, x.Value));

                if (spawnReason == CharacterClassManager.SpawnReason.Escaped)
                    eventargs.EscapeItems = player.Inventory.Items;

                if (classid != RoleType.Spectator && !lite)
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
                        eventargs.Position = __instance.DeathPosition;
                    }
                }

                Server.Get.Events.Player.InvokeSetClassEvent(eventargs);
                classid = eventargs.Role;
                if (eventargs.Allow)
                    player.setClassEventArgs = eventargs;

                return eventargs.Allow;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerSetClass(first) failed!!\n{e}");
                return true;
            }
        }

    }

    [HarmonyPatch(typeof(InventoryItemProvider), nameof(InventoryItemProvider.RoleChanged))]
    internal static class SetClassSecondPatch
    {
        [HarmonyPrefix]
        private static bool SpawnItemsPatch(ReferenceHub ply, RoleType prevRole, RoleType newRole, CharacterClassManager.SpawnReason spawnReason)
        {
            try
            {
                var player = ply.GetPlayer();

                if (spawnReason == CharacterClassManager.SpawnReason.Escaped)
                {
                    if (player.VanillaInventory.TryGetBodyArmor(out var bodyArmor))
                        bodyArmor.DontRemoveExcessOnDrop = true;

                    foreach (var item in player.setClassEventArgs.EscapeItems)
                        item.Drop();

                    InventoryItemProvider.PreviousInventoryPickups[ply] = player.setClassEventArgs.EscapeItems.Select(x => x.PickupBase).ToList();
                }
                else
                {
                    while (player.VanillaInventory.UserInventory.Items.Count > 0)
                        player.VanillaInventory.ServerRemoveItem(player.VanillaInventory.UserInventory.Items.ElementAt(0).Key, null);

                    player.VanillaInventory.UserInventory.ReserveAmmo.Clear();
                    player.VanillaInventory.SendAmmoNextFrame = true;
                }

                if (StartingInventories.DefinedInventories.TryGetValue(newRole, out var inventoryRoleInfo))
                {
                    foreach (var pair in player.setClassEventArgs.Ammo)
                        player.AmmoBox[pair.Key] = pair.Value;

                    foreach (var item in player.setClassEventArgs.Items)
                    {
                        item.PickUp(player);

                        var northwoodevent = InventoryItemProvider.OnItemProvided;

                        if (northwoodevent != null)
                            northwoodevent(ply, item.ItemBase);
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerSetClass(spawnitems) failed!!\n{e}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.ApplyProperties))]
    internal static class SetClassThirdPatch
    {
        [HarmonyPrefix]
        private static bool PositionPatch(CharacterClassManager __instance, bool lite = false)
        {
            try
            {
                var player = __instance.GetPlayer();
                var role = __instance.CurRole;

                if (!__instance._wasAnytimeAlive && __instance.CurClass != RoleType.Spectator && __instance.CurClass != RoleType.None)
                    __instance._wasAnytimeAlive = true;

                __instance.InitSCPs();
                __instance.AliveTime = 0f;

                if (role.team - Team.RSC <= 1)
                    __instance.EscapeStartTime = (int)Time.realtimeSinceStartup;

                try
                {
                    player.FootstepSync.SetLoudness(role.team, role.roleId.Is939());
                }
                catch { }

                if (role.roleId != RoleType.Spectator && RespawnManager.CurrentSequence() != RespawnManager.RespawnSequencePhase.SpawningSelectedTeam && UnitNamingManager.RolesWithEnforcedDefaultName.TryGetValue(role.roleId, out var spawnableTeamType) && RespawnManager.Singleton.NamingManager.TryGetAllNamesFromGroup((byte)spawnableTeamType, out var array) && array.Length != 0)
                {
                    __instance.NetworkCurSpawnableTeamType = (byte)spawnableTeamType;
                    __instance.NetworkCurUnitName = array[0];
                }
                else if (__instance.CurSpawnableTeamType != 0)
                {
                    __instance.NetworkCurSpawnableTeamType = 0;
                    __instance.NetworkCurUnitName = string.Empty;
                }

                if(role.roleId != RoleType.Spectator)
                {
                    if (!lite)
                    {
                        var args = player.setClassEventArgs;
                        //It's null when a person is revived by SCP049
                        if (args == null)
                        {
                            __instance._pms.OnPlayerClassChange(__instance.DeathPosition, 0f);
                        }
                        else
                        {
                            __instance._pms.OnPlayerClassChange(args.Position, args.Rotation);
                            __instance._pms.IsAFK = true;
                        }

                        if(!__instance.SpawnProtected && CharacterClassManager.EnableSP && CharacterClassManager.SProtectedTeam.Contains((int)role.team))
                        {
                            __instance.GodMode = true;
                            __instance.SpawnProtected = true;
                            __instance.ProtectedTime = Time.time;
                        }
                    }
                    if (!__instance.isLocalPlayer)
                        player.MaxHealth = role.maxHP;
                }

                __instance.Scp0492.iAm049_2 = (__instance.CurClass == RoleType.Scp0492);
                __instance.Scp106.iAm106 = (__instance.CurClass == RoleType.Scp106);
                __instance.Scp939.iAm939 = __instance.CurClass.Is939();
                __instance.RefreshPlyModel(RoleType.None);

                return false;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerSetClass(position) failed!!\n{e}");
                return true;
            }
        }
    }
}
