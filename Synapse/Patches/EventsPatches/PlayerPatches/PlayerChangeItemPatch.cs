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

				bool flag = (__instance.UserInventory.Items.TryGetValue(__instance.CurItem.SerialNumber, out var oldItem) && __instance.CurInstance != null) || __instance.CurItem.SerialNumber == 0;
				if (__instance.UserInventory.Items.TryGetValue(itemSerial, out var newItem) || itemSerial == 0)
				{
					if (__instance.CurItem.SerialNumber > 0 && flag && !oldItem.CanHolster()) return false;

					if (itemSerial != 0 && !newItem.CanEquip()) return false;

					if (itemSerial == 0)
					{
						var player = __instance._hub.GetPlayer();
						Server.Get.Events.Player.InvokeChangeItem(player, player.ItemInHand, SynapseItem.None, out var allow);

						if (!allow) return false;

						__instance.NetworkCurItem = InventorySystem.Items.ItemIdentifier.None;
						if (!__instance.isLocalPlayer)
						{
							__instance.CurInstance = null;
							return false;
						}
					}
					else
					{
						var player = __instance._hub.GetPlayer();
						Server.Get.Events.Player.InvokeChangeItem(player, player.ItemInHand, newItem.GetSynapseItem(), out var allow);

						if (!allow) return false;

						__instance.NetworkCurItem = new InventorySystem.Items.ItemIdentifier(newItem.ItemTypeId, itemSerial);
						if (!__instance.isLocalPlayer)
						{
							__instance.CurInstance = newItem;
							return false;
						}
					}
				}
				else if (!flag)
				{
					var player = __instance._hub.GetPlayer();
					Server.Get.Events.Player.InvokeChangeItem(player, player.ItemInHand, SynapseItem.None, out var allow);

					if (!allow) return false; 

					__instance.NetworkCurItem = InventorySystem.Items.ItemIdentifier.None;
					if (!__instance.isLocalPlayer) __instance.CurInstance = null;
				}

                return true;
            }
            catch(Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerChangeItem event failed!!\n{e}");
                return true;
            }
        }
    }
}
