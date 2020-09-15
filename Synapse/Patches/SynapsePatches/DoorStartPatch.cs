using Harmony;
using Synapse.Api;
using System.Linq;

// ReSharper disable All
namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(Door), nameof(Door.Start))]
    internal class DoorStartPatch
    {
        private static void Postfix(Door __instance)
        {
            while (Map.Get.Doors.Select(x => x.GameObject).Contains(null))
                Map.Get.Doors.Remove(Map.Get.Doors.FirstOrDefault(x => x.GameObject == null));

            SynapseController.Server.Map.Doors.Add(new Api.Door(__instance));
        }
    }
}
