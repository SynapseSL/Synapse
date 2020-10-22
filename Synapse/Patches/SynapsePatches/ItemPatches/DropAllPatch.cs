using System.Linq;
using Harmony;

namespace Synapse.Patches.SynapsePatches.ItemPatches
{
    [HarmonyPatch(typeof(Inventory),nameof(Inventory.ServerDropAll))]
    internal static class DropAllPatch
    {
        private static void Prefix(Inventory __instance)
        {
            foreach (var item in __instance.items.ToList())
                if (item.GetSynapseItem() != null)
                    item.GetSynapseItem().Drop();
        }
    }
}
