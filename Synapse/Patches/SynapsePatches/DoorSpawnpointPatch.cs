using HarmonyLib;
using MapGeneration;
using Synapse.Api;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(DoorSpawnpoint), nameof(DoorSpawnpoint.Start))]
    public class DoorSpawnpointPatch
    {
        private static bool Prefix(DoorSpawnpoint __instance)
        {
            try
            {
                if (Map.DoorVariantPrefab == null)
                    Map.DoorVariantPrefab = UnityEngine.Object.Instantiate(__instance.TargetPrefab);
            }
            catch (System.Exception e)
            {
                Logger.Get.Error($"Synapse-DoorSpawnpoint: DoorSpawnpoint Start Patch failed!!\n{e}");
            }
            return true;
        }
    }
}
