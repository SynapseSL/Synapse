using Harmony;

namespace Synapse.Patches.SynapsePatches.Item_Patches
{
    [HarmonyPatch(typeof(Inventory.SyncListItemInfo), nameof(Inventory.SyncListItemInfo.ModifyDuration))]
    internal static class ModifyDurationPatch
    {
        private static bool Prefix(Inventory.SyncListItemInfo __instance,int index, float value)
        {
            var item = __instance[index].GetItem();
            if(item != null)
            {
                item.Durabillity = value;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Inventory.SyncListItemInfo), nameof(Inventory.SyncListItemInfo.ModifyAttachments))]
    internal static class ModifyAttachmentsPatch
    {
        private static bool Prefix(Inventory.SyncListItemInfo __instance, int index, int s, int b, int o)
        {
            var item = __instance[index].GetItem();
            if (item != null)
            {
                item.Sight = s;
                item.Barrel = b;
                item.Other = o;
                return false;
            }
            return true;
        }
    }
}
