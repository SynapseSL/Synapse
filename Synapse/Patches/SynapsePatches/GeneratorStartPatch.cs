using Harmony;
using Synapse.Api;
using System.Linq;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(Generator079), nameof(Generator079.Awake))]
    internal static class GeneratorStartPatch
    {
        private static void Postfix(Generator079 __instance)
        {
            while (Map.Get.Generators.Select(x => x.GameObject).Contains(null))
                Map.Get.Doors.Remove(Map.Get.Doors.FirstOrDefault(x => x.GameObject == null));

            var generator = new Api.Generator(__instance);

            if (generator.Name.Contains("("))
            {
                Map.Get.MainGenerator = generator;
                return;
            }

            Map.Get.Generators.Add(new Api.Generator(__instance));
        }
    }
}
