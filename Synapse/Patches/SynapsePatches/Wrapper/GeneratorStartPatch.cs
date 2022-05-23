using System.Linq;
using HarmonyLib;
using MapGeneration.Distributors;
using Synapse.Api;
namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(Scp079Generator), nameof(Scp079Generator.Start))]
    internal static class GeneratorStartPatch
    {
        [HarmonyPostfix]
        private static void Start(Scp079Generator __instance)
        {
            while (Map.Get.Generators.Select(x => x.GameObject).Contains(null))
                Map.Get.Doors.Remove(Map.Get.Doors.FirstOrDefault(x => x.GameObject is null));

            Map.Get.Generators.Add(new(__instance));
        }
    }
}