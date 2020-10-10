using Harmony;
using System.Linq;

namespace Synapse.Patches.SynapsePatches.Item_Patches
{
    [HarmonyPatch(typeof(Inventory),nameof(Inventory.ServerDropAll))]
    internal static class DropAllPatch
    {
        private static void Prefix(Inventory __instance)
        {
            foreach (var item in __instance.items.ToList())
                if (item.GetItem() != null)
                    item.GetItem().Drop();
        }
    }
}
