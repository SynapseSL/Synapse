using Harmony;
using Synapse.Api.Components;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(Lift), nameof(Lift.Start))]
    internal static class ElevatorStartPatch
    {
        private static void Postfix(Lift __instance)
        {
            while (SynapseController.Server.Map.Elevators.Contains(null))
                SynapseController.Server.Map.Elevators.Remove(null);

            SynapseController.Server.Map.Elevators.Add(new Elevator(__instance));
        }
    }
}
