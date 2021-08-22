using HarmonyLib;
using InventorySystem.Items.Pickups;
using Synapse.Api.Items;

namespace Synapse.Patches.SynapsePatches.Item
{
    [HarmonyPatch(typeof(ItemPickupBase), nameof(ItemPickupBase.DestroySelf))]
    internal static class PickupPatches
    {
        [HarmonyPostfix]
        private static void DestroySelfPatch(ItemPickupBase __instance)
        {
            var item = SynapseItem.AllItems[__instance.Info.Serial];
            //Whenever the Item should be transformed to a Inventory Item a ItemBase will be created before
            //so that when ItemBase null is the game wants to destroy it
            if (item.ItemBase == null)
                item.Destroy();
        }
    }
}
