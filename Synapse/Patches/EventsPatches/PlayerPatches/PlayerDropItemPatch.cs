using System;
using HarmonyLib;
using InventorySystem;
using Synapse.Api;
using Synapse.Api.Items;
using InventorySystem.Items;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.UserCode_CmdDropItem))]
    internal static class DropItemPatch
    {
        [HarmonyPrefix]
        private static bool PlayerDropItemPatch(Inventory __instance, ushort itemSerial, ref bool tryThrow)
        {
            try
            {
                if (!__instance.UserInventory.Items.TryGetValue(itemSerial, out var itembase) || !itembase.CanHolster())
                    return false;

                var item = itembase.GetSynapseItem();

                Server.Get.Events.Player.InvokePlayerDropItemPatch(__instance.GetPlayer(), item, ref tryThrow, out var allow);

                return allow;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: DropItem failed!!\n{e}");
                return true;
            }
        }
    }
}
