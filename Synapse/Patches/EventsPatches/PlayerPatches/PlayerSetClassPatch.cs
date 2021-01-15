using System;
using System.Collections.Generic;
using HarmonyLib;
using Mirror;
using Synapse.Api.Events.SynapseEventArguments;
using Synapse.Api.Items;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(CharacterClassManager),nameof(CharacterClassManager.SetPlayersClass))]
    internal static class PlayerSetClassPatch
    {
        private static bool Prefix(CharacterClassManager __instance, ref PlayerSetClassEventArgs __state, ref RoleType classid, GameObject ply, ref bool escape,bool lite)
        {
            try
            {
                if (!NetworkServer.active || ply == null) return false;
                var player = ply.GetPlayer();
                if (player.Hub.isDedicatedServer || !player.Hub.Ready) return false;

                __state = new PlayerSetClassEventArgs
                {
                    EscapeItems = new List<SynapseItem>(),
                    IsEscaping = escape,
                    Allow = true,
                    Player = player,
                    Role = classid,
                    Items = new List<SynapseItem>(),
                    Position = Vector3.zero,
                    Rotation = 0f
                };

                if (escape && CharacterClassManager.KeepItemsAfterEscaping && !lite)
                    __state.EscapeItems = player.Inventory.Items;

                if (!lite)
                    foreach (var id in __instance.Classes.SafeGet(classid).startItems)
                    {
                        var synapseitem = new SynapseItem(id, 0, 0, 0, 0);
                        var item = new Item(player.VanillaInventory.GetItemByID(id));
                        synapseitem.Durabillity = item.durability;

                        for (int i = 0; i < player.VanillaInventory._weaponManager.weapons.Length; i++)
                        {
                            if (player.VanillaInventory._weaponManager.weapons[i].inventoryID == id)
                            {
                                synapseitem.Sight = player.VanillaInventory._weaponManager.modPreferences[i, 0];
                                synapseitem.Barrel = player.VanillaInventory._weaponManager.modPreferences[i, 1];
                                synapseitem.Other = player.VanillaInventory._weaponManager.modPreferences[i, 2];
                            }
                        }

                        __state.Items.Add(synapseitem);
                    }

                Synapse.Api.Logger.Get.Warn("Pos finden");
                if (__instance.Classes.SafeGet(classid).team != Team.RIP)
                {
                    Synapse.Api.Logger.Get.Warn("kein Spec");
                    if (lite)
                    {
                        __state.Position = player.Position;
                        Synapse.Api.Logger.Get.Warn("lite");
                    }
                    else
                    {
                        if (Synapse.Api.Map.Get.RespawnPoint != Vector3.zero)
                        {
                            __state.Position = Synapse.Api.Map.Get.RespawnPoint;
                            Synapse.Api.Logger.Get.Warn("constant");
                        }
                        else
                        {
                            Synapse.Api.Logger.Get.Warn("spawnpoint suchen");
                            var randomPosition = CharacterClassManager._spawnpointManager.GetRandomPosition(classid);
                            if (randomPosition != null)
                            {
                                Synapse.Api.Logger.Get.Warn("spawnpoint gefunden");
                                __state.Position = randomPosition.transform.position;
                                __state.Rotation = randomPosition.transform.rotation.eulerAngles.y;
                            }
                            else
                                __state.Position = player.DeathPosition;
                        }
                    }
                }

                try
                {
                    Server.Get.Events.Player.InvokeSetClassEvent(__state);
                }
                catch (Exception e)
                {
                    Logger.Get.Error($"Synapse-Event: PlayerSetClass failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                }

                if (!__state.Allow) return false;
                classid = __state.Role;

                foreach (var item in __state.EscapeItems)
                    item.Despawn();

                //WHY THE FUCK DOES SCP NOT USE THEIR OWN METHODS TO CLEAR THE INVENTORY THAT I ALREADY PATCHED?
                player.Inventory.Clear();

                player.spawnPosition = __state.Position;
                player.spawnRotation = __state.Rotation;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerSetClass Prefix failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }
            return true;
        }

        private static void Postfix(CharacterClassManager __instance, PlayerSetClassEventArgs __state,RoleType classid, bool lite)
        {
            try
            {
                if (lite) return;
                if (__state == null) return;
                if (!__state.Allow) return;

                var player = __state.Player;

                player.Inventory.Clear();
                var role = player.ClassManager.Classes.SafeGet(classid);
                if (role.roleId != RoleType.Spectator)
                {
                    player.Ammo5 = role.ammoTypes[0];
                    player.Ammo7 = role.ammoTypes[1];
                    player.Ammo9 = role.ammoTypes[2];
                }
                foreach (var item in __state.Items)
                    player.Inventory.AddItem(item);

                if (__state.EscapeItems.Count == 0) return;
                foreach (var item in __state.EscapeItems)
                {
                    if (CharacterClassManager.PutItemsInInvAfterEscaping)
                    {
                        var itemByID = player.VanillaInventory.GetItemByID(item.ItemType);
                        var flag = false;
                        var categories = __instance._search.categories;
                        int i = 0;
                        while (i < categories.Length)
                        {
                            var invcategorie = categories[i];
                            if (invcategorie.itemType == itemByID.itemCategory && itemByID.itemCategory != ItemCategory.None)
                            {
                                int num = 0;
                                foreach (var sync in player.VanillaInventory.items)
                                    if (player.VanillaInventory.GetItemByID(sync.id).itemCategory == itemByID.itemCategory)
                                        num++;

                                if (num >= (int)invcategorie.maxItems)
                                {
                                    flag = true;
                                    break;
                                }
                                break;
                            }
                            else
                                i++;
                        }

                        if (player.VanillaInventory.items.Count >= 8 || (flag && !item.IsCustomItem))
                            item.Drop(__instance._pms.RealModelPosition);
                        else
                            item.PickUp(player);
                    }
                    else
                        item.Drop(__instance._pms.RealModelPosition);
                }
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerSetClass Postfix failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager),nameof(CharacterClassManager.ApplyProperties))]
    internal static class PositionPatch
    {
        private static bool Prefix(CharacterClassManager __instance, bool lite, bool escape)
        {
            try
            {
                var player = __instance.GetPlayer();
                var curRole = __instance.CurRole;
                if (!__instance._wasAnytimeAlive && player.RoleType != RoleType.Spectator && player.RoleType != RoleType.None)
                    __instance._wasAnytimeAlive = true;

                __instance.InitSCPs();
                __instance.AliveTime = 0f;
                if (player.Team - Team.RSC <= 1)
                    __instance.EscapeStartTime = (int)Time.realtimeSinceStartup;
                try
                {
                    __instance._hub.footstepSync.SetLoudness(curRole.team, curRole.roleId.Is939());
                }
                catch
                {
                }
                if (NetworkServer.active)
                {
                    player.Handcuffs.ClearTarget();
                    player.Handcuffs.NetworkCufferId = -1;
                    player.Handcuffs.NetworkForceCuff = false;
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
                }
                if(player.Team != Team.RIP)
                {
                    if(NetworkServer.active && !lite)
                    {
                        if (__instance.CurClass == RoleType.Scp0492) player.spawnPosition = player.DeathPosition;
                        player.PlayerMovementSync.OnPlayerClassChange(player.spawnPosition, player.spawnRotation);
                        player.PlayerMovementSync.IsAFK = true;
                        if(escape && CharacterClassManager.KeepItemsAfterEscaping)
                        {
                            for (var num = 0; num < 3; num++)
                                if (player.AmmoBox[num] >= 15)
                                {
                                    var item = new SynapseItem(player.AmmoBox.types[num].inventoryID, player.AmmoBox[num], 0, 0, 0);
                                    item.Drop(player.spawnPosition);
                                }
                        }
                        player.AmmoBox.ResetAmmo();

                        if(!__instance.SpawnProtected && CharacterClassManager.EnableSP && CharacterClassManager.SProtectedTeam.Contains((int)curRole.team))
                        {
                            __instance.GodMode = true;
                            __instance.SpawnProtected = true;
                            __instance.ProtectedTime = Time.time;
                        }
                    }
                    if (!__instance.isLocalPlayer)
                        player.MaxHealth = curRole.maxHP;
                }

                __instance.Scp0492.iAm049_2 = __instance.CurClass == RoleType.Scp0492;
                __instance.Scp106.iAm106 = __instance.CurClass == RoleType.Scp106;
                __instance.Scp173.iAm173 = __instance.CurClass == RoleType.Scp173;
                __instance.Scp939.iAm939 = __instance.CurClass.Is939();
                __instance.RefreshPlyModel(RoleType.None);
                return false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerSetClass(position) failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}
