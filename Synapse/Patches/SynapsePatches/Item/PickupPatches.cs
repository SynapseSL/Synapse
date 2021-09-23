using HarmonyLib;
using InventorySystem.Items.Pickups;
using Synapse.Api;
using Synapse.Api.Items;

namespace Synapse.Patches.SynapsePatches.Item
{
    //[HarmonyPatch(typeof(ItemPickupBase), nameof(ItemPickupBase.DestroySelf))]
    internal static class PickupPatches
    {
        [HarmonyPrefix]
        private static bool DestroySelfPatch(ItemPickupBase __instance)
        {
            if (!SynapseItem.AllItems.TryGetValue(__instance.Info.Serial, out var item))
            {
                Logger.Get.Warn($"Found unregistered ItemSerial: {__instance.Info.Serial}");
                return false;
            }
            //Whenever the Item should be transformed to a Inventory Item a ItemBase will be created before
            //so that when ItemBase null is the game wants to destroy it
            if (item.ItemBase == null)
            {
                item.Destroy();
                return false;
            }

            return true;
        }
    }
}
