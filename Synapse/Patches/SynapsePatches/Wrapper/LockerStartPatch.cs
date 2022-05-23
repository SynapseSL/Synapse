using HarmonyLib;
using MapGeneration.Distributors;
namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(Locker), nameof(Locker.Start))]
    internal static class LockerStartPatch
    {
        [HarmonyPostfix]
        private static void Start(Locker __instance)
        {
            SynapseController.Server.Map.Lockers.Add(new(__instance));
        }
    }
}