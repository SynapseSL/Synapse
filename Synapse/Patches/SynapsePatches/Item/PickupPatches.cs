using HarmonyLib;
using InventorySystem.Items.Pickups;

namespace Synapse.Patches.SynapsePatches.Item
{
    [HarmonyPatch(typeof(ItemPickupBase), nameof(ItemPickupBase.DestroySelf))]
    internal static class PickupPatches
    {
        [HarmonyPrefix]
        private static bool DestroySelfPatch(ItemPickupBase __instance)
        {
            var item = __instance.GetSynapseItem();
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
