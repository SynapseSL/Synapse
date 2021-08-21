using System;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items;
using Synapse.Api.Items;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(Inventory),nameof(Inventory.ServerSelectItem))]
    internal static class PlayerChangeItemPatch
    {
        private static bool Prefix(Inventory __instance, ushort itemSerial)
        {
            try
            {
                if (itemSerial == __instance.CurItem.SerialNumber) return false;

                var player = __instance.GetPlayer();
                var olditem = player.ItemInHand;
                var newitem = itemSerial == 0 ? null : SynapseItem.AllItems[itemSerial];

                if (newitem != null && (!olditem.ItemBase.CanHolster() || !newitem.ItemBase.CanEquip())) return false;

                Server.Get.Events.Player.InvokeChangeItem(player, olditem, newitem);

                return false;
            }
            catch(Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerChangeItem event failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}
