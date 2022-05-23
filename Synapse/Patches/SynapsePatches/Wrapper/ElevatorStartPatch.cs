using HarmonyLib;
namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(Lift), nameof(Lift.Start))]
    internal static class ElevatorStartPatch
    {
        [HarmonyPostfix]
        private static void Start(Lift __instance)
        {
            SynapseController.Server.Map.Elevators.Add(new(__instance));
        }
    }
}