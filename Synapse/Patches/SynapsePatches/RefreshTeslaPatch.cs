using Harmony;
using Synapse.Api.Components;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(TeslaGateController), nameof(TeslaGateController.RefreshTeslaGates))]
    internal static class RefreshTeslaPatch
    {
        private static void Postfix(TeslaGateController __instance)
        {
            SynapseController.Server.Map.Teslas.Clear();
            foreach (var tesla in __instance.TeslaGates)
                SynapseController.Server.Map.Teslas.Add(new Tesla(tesla));
        }
    }
}
