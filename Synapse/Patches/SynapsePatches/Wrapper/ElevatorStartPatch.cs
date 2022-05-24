using HarmonyLib;
using Synapse.Api;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(Lift), nameof(Lift.Start))]
    internal static class ElevatorStartPatch
    {
        [HarmonyPostfix]
        private static void Start(Lift __instance) => SynapseController.Server.Map.Elevators.Add(new Elevator(__instance));
    }
}
