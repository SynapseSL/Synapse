using Harmony;
using System.Linq;

namespace Synapse.Patches.SynapsePatches.Item_Patches
{
    [HarmonyPatch(typeof(Inventory),nameof(Inventory.Clear))]
    internal static class InventoryClearPatch
    {
        private static void Prefix(Inventory __instance)
        {
            foreach (var item in __instance.items.ToList())
                if (item.GetSynapseItem() != null)
                    item.GetSynapseItem().Destroy();
        }
    }
}
