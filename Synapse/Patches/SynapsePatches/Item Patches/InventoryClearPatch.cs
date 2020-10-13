using Harmony;
using Synapse.Api;

namespace Synapse.Patches.SynapsePatches.Item_Patches
{
    [HarmonyPatch(typeof(Inventory),nameof(Inventory.Clear))]
    internal static class InventoryClearPatch
    {
        private static void Prefix(Inventory __instance)
        {
            foreach (var item in __instance.GetPlayer().Inventory.Items)
                item.Destroy();
        }
    }
}
