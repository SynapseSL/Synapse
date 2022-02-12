using HarmonyLib;
using InventorySystem;
using MEC;
using PlayerStatsSystem;
using Synapse.Api;
using Synapse.Api.Enum;
using Synapse.Api.Events.SynapseEventArguments;
using Synapse.Api.Items;
using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetPlayersClass))]
    internal static class SetPlayersClassPatch
    {
        [HarmonyPrefix]
        private static bool OnSetClass(ref RoleType classid, GameObject ply, CharacterClassManager.SpawnReason spawnReason)
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
                    var randomPosition = SpawnpointManager.GetRandomPosition(classid);
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
                    {
                        eventargs.Items.Add(new SynapseItem(itemtype));
                    }
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

        [HarmonyPostfix]
        private static void RemoveArgs(CharacterClassManager __instance) => __instance.GetPlayer().setClassEventArgs = null;
    }

    [HarmonyPatch(typeof(PlayerMovementSync),nameof(PlayerMovementSync.OnPlayerClassChange))]
    internal static class HandlePositionPatch
    {
        [HarmonyPrefix]
        private static bool OnPlayerMovementSync(PlayerMovementSync __instance)
        {
            try
            {
                var player = __instance.GetPlayer();
                var args = player.setClassEventArgs;
                //It is null when someone is revived by 049 since the first patch is never called in this situation
                if (args == null) return true;
                Timing.RunCoroutine(__instance.SafelySpawnPlayer(args.Position, args.Rotation), Segment.FixedUpdate);
                return false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerSetClass(Position) failed!!\n{e}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(InventoryItemProvider), nameof(InventoryItemProvider.RoleChanged))]
    internal static class HandleItemPatch
    {
        [HarmonyPrefix]
        private static bool OnRoleChanged(ReferenceHub ply)
        {
            try
            {
                var player = ply.GetPlayer();
                var args = player.setClassEventArgs;

                //If args is null he is SCP0492 and should not get any Items
                if (args == null) return false;

                var inventory = ply.inventory;

                if (args.IsEscaping) foreach (var item in player.Inventory.Items) item.Despawn();
                else player.Inventory.Clear();

                foreach (var ammo in args.Ammo)
                    player.AmmoBox[ammo.Key] = ammo.Value;

                foreach (var item in args.Items)
                {
                    item.PickUp(player);
                    InventoryItemProvider.OnItemProvided?.Invoke(player.Hub, item.ItemBase);
                }

                if(args.IsEscaping) foreach(var item in args.EscapeItems) item.PickUp(player);

                return false;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerSetClass(Items) failed!!\n{e}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(HealthStat),nameof(HealthStat.ClassChanged))]
    internal static class HandleHealthPatch
    {
        [HarmonyPrefix]
        private static void OnClassChanged(HealthStat __instance)
        {
            try
            {
                var player = __instance.GetPlayer();
                player.MaxHealth = player.ClassManager.CurRole.maxHP;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerSetClass(Health) failed!!\n{e}");
            }
        }
    }
}
