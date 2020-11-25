using System;
using HarmonyLib;
using UnityEngine;
using Event = Synapse.Api.Events.EventHandler;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.SynapsePatches.ItemPatches
{
    [HarmonyPatch(typeof(WorkStation),nameof(WorkStation.ConnectTablet))]
    internal static class WorkStationPatch
    {
        private static bool Prefix(WorkStation __instance, GameObject tabletOwner)
        {
            try
            {
                if (!__instance.CanPlace(tabletOwner) || __instance._animationCooldown > 0f) return false;

                var player = tabletOwner.GetPlayer();
                var station = __instance.GetWorkStation();

                foreach (var item in player.Inventory.Items)
                    if (item.ItemType == ItemType.WeaponManagerTablet)
                    {
                        try
                        {
                            Event.Get.Player.InvokePlayerConnectWorkstation(player, item,station, out var allow);
                            Event.Get.Player.InvokePlayerItemUseEvent(player, item, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allow);

                            if (!allow) continue;
                        }
                        catch(Exception e)
                        {
                            Synapse.Api.Logger.Get.Error($"Synapse-Event: PlayerConnectWorkstation Event failed!!\n{e}");
                        }

                        item.Despawn();
                        station.ConnectedTablet = item;
                        station.TabletOwner = tabletOwner.GetPlayer();
                    }
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Item: Connect Workstation Tablet failed!!\n{e}");
            }
            
            return false;
        }
    }

    [HarmonyPatch(typeof(WorkStation), nameof(WorkStation.UnconnectTablet))]
    internal static class WorkStationPatch2
    {
        private static bool Prefix(WorkStation __instance, GameObject taker)
        {
            try
            {
                if (!__instance.CanTake(taker) || __instance._animationCooldown > 0f) return false;

                var player = taker.GetPlayer();
                var station = __instance.GetWorkStation();

                try
                {
                    Event.Get.Player.InvokePlayerUnonnectWorkstation(player,station, out var allow);

                    if (!allow) return false;
                }
                catch(Exception e)
                {
                    Synapse.Api.Logger.Get.Error($"Synapse-Event: PlayerConnectWorkstation Event failed!!\n{e}");
                }

                station.TabletOwner = player;
                station.IsTabletConnected = false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Item: Disconnect Workstation Tablet failed!!\n{e}");
            }
            return false;
        }
    }
}
