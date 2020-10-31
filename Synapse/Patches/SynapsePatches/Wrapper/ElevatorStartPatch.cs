using HarmonyLib;
using Synapse.Api;
using System.Linq;

// ReSharper disable All
namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(Lift), nameof(Lift.Start))]
    internal static class ElevatorStartPatch
    {
        private static void Postfix(Lift __instance)
        {
            while (Map.Get.Elevators.Select(x => x.GameObject).Contains(null))
                Map.Get.Elevators.Remove(Map.Get.Elevators.FirstOrDefault(x => x.GameObject == null));

            SynapseController.Server.Map.Elevators.Add(new Elevator(__instance));
        }
    }
}
