using HarmonyLib;

namespace Synapse.Patches.SynapsePatches.ItemPatches
{
    //TODO: Implement a Patch like this when Grenades are implemented
    /*
    [HarmonyPatch(typeof(FragGrenade),nameof(FragGrenade.ChangeIntoGrenade))]
    internal static class ChangeGrenadePatch
    {
        private static void Prefix(Pickup item)
        {
            var sitem = item?.GetSynapseItem();
            if (sitem != null) Synapse.Api.Map.Get.Items.Remove(sitem);
        }
    }
    */
}
