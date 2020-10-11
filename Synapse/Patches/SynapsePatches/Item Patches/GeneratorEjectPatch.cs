using Harmony;
using Synapse.Api;

namespace Synapse.Patches.SynapsePatches.Item_Patches
{
    [HarmonyPatch(typeof(Generator079),nameof(Generator079.EjectTablet))]
    internal static class GeneratorEjectPatch
    {
        private static bool Prefix(Generator079 __instance)
        {
            if (__instance.isTabletConnected)
            {
                var gen = __instance.GetGenerator();
                __instance.NetworkisTabletConnected = false;
                if (gen.ConnectedTabled != null)
                {
                    Map.Get.Items.Add(gen.ConnectedTabled);
                    gen.ConnectedTabled.Drop(gen.TabletEjectionPoint);
                }
            }
            return false;
        }
    }
}
