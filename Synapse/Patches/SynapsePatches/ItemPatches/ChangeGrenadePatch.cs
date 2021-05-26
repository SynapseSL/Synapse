using HarmonyLib;
using Grenades;

namespace Synapse.Patches.SynapsePatches.ItemPatches
{
    [HarmonyPatch(typeof(FragGrenade),nameof(FragGrenade.ChangeIntoGrenade))]
    internal static class ChangeGrenadePatch
    {
        private static void Prefix(Pickup item)
        {
            var sitem = item?.GetSynapseItem();
            if (sitem != null) Synapse.Api.Map.Get.Items.Remove(sitem);
        }
    }
}
