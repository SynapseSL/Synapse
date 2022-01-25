using HarmonyLib;
using Synapse.Api;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(MapGeneration.Distributors.Locker), nameof(MapGeneration.Distributors.Locker.Start))]
    internal static class LockerStartPatch
    {
        [HarmonyPostfix]
        private static void Start(MapGeneration.Distributors.Locker __instance)
        {
            SynapseController.Server.Map.Lockers.Add(new Locker(__instance));
        }
    }
}