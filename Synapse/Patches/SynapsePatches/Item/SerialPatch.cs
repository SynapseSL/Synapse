using HarmonyLib;
using Synapse.Api.Items;
namespace Synapse.Patches.SynapsePatches.Item
{
    [HarmonyPatch(typeof(InventorySystem.Items.ItemSerialGenerator), nameof(InventorySystem.Items.ItemSerialGenerator.GenerateNext))]
    internal static class GenerateSerialPatch
    {
        [HarmonyPostfix]
        private static void GeneratePatch(ushort __result) => SynapseItem.AllItems[__result] = null;
    }
}