using System;
using System.Linq;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Configs;
using InventorySystem.Items.Usables;
using MEC;
using Neuron.Core.Logging;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Teams.Unit;
using UnityEngine;

namespace Synapse3.SynapseModule.Patches.PlayerPatches;

[Patches]
[HarmonyPatch]
internal static class SetClassPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetPlayersClass))]
    public static bool OnSetClass(ref RoleType classid, GameObject ply, CharacterClassManager.SpawnReason spawnReason)
    {
        try
        {
            var player = ply.GetSynapsePlayer();

            if (player.PlayerType == PlayerType.Server || !player.Hub.Ready)
                return false;

            var ev = new SetClassEvent(player, classid, spawnReason);

            if (spawnReason == CharacterClassManager.SpawnReason.Escaped)
                ev.EscapeItems = player.Inventory.Items.ToList();

            if (classid != RoleType.Spectator && classid != RoleType.None)
            {
                var randomPosition = SpawnpointManager.GetRandomPosition(classid);
                var global = Synapse.Get<MapService>().GlobalRespawnPoint;
                if (global != Vector3.zero)
                {
                    ev.Position = global;
                }
                else if (randomPosition != null)
                {
                    ev.Position = randomPosition.transform.position;
                    ev.Rotation =
                        new PlayerMovementSync.PlayerRotation(0f, randomPosition.transform.rotation.eulerAngles.y);
                }
                else
                {
                    ev.Position = player.DeathPosition;
                }
            }

            if (StartingInventories.DefinedInventories.TryGetValue(classid, out var roleItems))
            {
                foreach (var ammo in roleItems.Ammo)
                {
                    ev.Ammo[(AmmoType)ammo.Key] = ammo.Value;
                }

                foreach (var itemType in roleItems.Items)
                {
                    if (itemType == ItemType.None) continue;
                    ev.Items.Add((uint)itemType);
                }
            }

            var unitInfo = Synapse.Get<UnitService>()
                .GetPlayerUnit(player, player.CustomRole?.Attribute?.Id ?? (uint)classid);
            ev.UnitId = unitInfo.UnitId;
            ev.Unit = unitInfo.UnitName;

            Synapse.Get<PlayerEvents>().SetClass.Raise(ev);

            classid = ev.Role;

            if (ev.Allow)
                player.setClassStored = ev;

            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlayerSetClass Patch failed\n" + ex);
            return true;
        }
    }
    

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetPlayersClass))]
    public static void AfterSetClass(GameObject ply)
    {
        var player = ply.GetSynapsePlayer();
        if (player != null)
            player.setClassStored = null;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.OnPlayerClassChange))]
    public static bool HandlePosition(PlayerMovementSync __instance)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            var args = player.setClassStored;

            if (player.LiteRoleSet)
                return false;

            //SCP-049-2 Does not use SetPlayersClass therefore args are null for them
            if (args == null)
                return true;

            Timing.RunCoroutine(__instance.SafelySpawnPlayer(args.Position, args.Rotation));
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlayerSetClass(Position) Patch failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventoryItemProvider), nameof(InventoryItemProvider.RoleChanged))]
    public static bool HandleItem(ReferenceHub ply)
    {
        try
        {
            var player = ply.GetSynapsePlayer();
            var args = player?.setClassStored;

            if (player?.LiteRoleSet == true)
                return false;
            
            player.Inventory.ClearAllItems();

            //This is the case when someone is revived as SCP-049-2 or set to OverWatch
            if (args == null) return false;
            
            foreach (var ammo in args.Ammo)
            {
                player.Inventory.AmmoBox[ammo.Key] = ammo.Value;
            }

            
            foreach (var itemId in args.Items)
            {
                var item = player.Inventory.GiveItem(itemId);
                InventoryItemProvider.OnItemProvided?.Invoke(player, item.Item);
            }

            if (args.SpawnReason == CharacterClassManager.SpawnReason.Escaped)
            {
                foreach (var item in args.EscapeItems)
                {
                    item.EquipItem(player);
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlayerSetClass(Item) Patch failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HealthStat), nameof(HealthStat.ClassChanged))]
    public static bool HandleHealth(HealthStat __instance)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (player.LiteRoleSet)
                return false;

            player.MaxHealth = player.ClassManager.CurRole.maxHP;
            return true;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlayerSetClass(Health) Patch failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UsableItemsController), nameof(UsableItemsController.ResetPlayerOnRoleChange))]
    public static bool HandleUsableItems(ReferenceHub ply)
    {
        try
        {
            if (ply?.GetSynapsePlayer().LiteRoleSet == true) return false;
            return true;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlayerSetClass(UsableItem) Patch failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerEffectsController), nameof(PlayerEffectsController.CharacterClassManager_OnClassChanged))]
    public static bool HandleEffects(ReferenceHub targetHub)
    {
        try
        {
            if (targetHub?.GetSynapsePlayer().LiteRoleSet == true) return false;
            return true;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlayerSetClass(Effects) Patch failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.ApplyProperties))]
    public static bool OnApplyProperties(CharacterClassManager __instance, ref bool lite, bool escape)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (player != null)
                lite = player.LiteRoleSet;
            
            var curRole = __instance.CurRole;
            if (!__instance._wasAnytimeAlive && __instance.CurClass != RoleType.Spectator &&
                __instance.CurClass != RoleType.None)
            {
                __instance._wasAnytimeAlive = true;
            }

            __instance.InitSCPs();
            __instance.AliveTime = 0f;
            var team = curRole.team;
            if (team - Team.RSC <= 1)
            {
                __instance.EscapeStartTime = (int)Time.realtimeSinceStartup;
            }

            try
            {
                __instance._hub.footstepSync.SetLoudness(curRole.team, curRole.roleId.Is939());
            }
            catch
            {
                // ignored
            }
            
            if (!lite)
            {
                if (player?.setClassStored != null)
                {
                    __instance.NetworkCurUnitName = player.setClassStored.Unit;
                    __instance.NetworkCurSpawnableTeamType = player.setClassStored.UnitId;
                }
                else if (player != null)
                {
                    var defaultUnit = Synapse.Get<UnitService>().GetPlayerUnit(player, player.RoleID);
                    __instance.NetworkCurUnitName = defaultUnit.UnitName;
                    __instance.NetworkCurSpawnableTeamType = defaultUnit.UnitId;
                }
                else
                {
                    __instance.NetworkCurUnitName = "";
                    __instance.NetworkCurSpawnableTeamType = 0;
                }   
            }

            if (curRole.team != Team.RIP)
            {
                if (!lite)
                {
                    Vector3 constantRespawnPoint = NonFacilityCompatibility.currentSceneSettings.constantRespawnPoint;
                    if (constantRespawnPoint != Vector3.zero)
                    {
                        __instance._pms.OnPlayerClassChange(constantRespawnPoint, null);
                        __instance._pms.IsAFK = true;
                    }
                    else
                    {
                        GameObject randomPosition = SpawnpointManager.GetRandomPosition(__instance.CurClass);
                        if (randomPosition != null)
                        {
                            __instance._pms.OnPlayerClassChange(randomPosition.transform.position,
                                new PlayerMovementSync.PlayerRotation?(new PlayerMovementSync.PlayerRotation(
                                    new float?(0f), new float?(randomPosition.transform.rotation.eulerAngles.y))));
                            __instance._pms.IsAFK = true;
                        }
                        else
                        {
                            __instance._pms.OnPlayerClassChange(__instance.DeathPosition, null);
                        }
                    }

                    if (!__instance.SpawnProtected && CharacterClassManager.EnableSP &&
                        CharacterClassManager.SProtectedTeam.Contains((int)curRole.team))
                    {
                        __instance.GodMode = true;
                        __instance.SpawnProtected = true;
                        __instance.ProtectedTime = Time.time;
                    }
                }
            }

            __instance.Scp0492.iAm049_2 = __instance.CurClass == RoleType.Scp0492;
            __instance.Scp106.iAm106 = __instance.CurClass == RoleType.Scp106;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlayerSetClass(Apply) Patch failed\n" + ex);
            return true;
        }

        return false;
    }
}