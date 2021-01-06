using HarmonyLib;
using Synapse.Api;
using System.Linq;
using Interactables.Interobjects.DoorUtils;

// ReSharper disable All
namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(DoorVariant), nameof(DoorVariant.Start))]
    internal class DoorStartPatch
    {
        private static void Postfix(DoorVariant __instance)
        {
            while (Map.Get.Doors.Select(x => x.GameObject).Contains(null))
                Map.Get.Doors.Remove(Map.Get.Doors.FirstOrDefault(x => x.GameObject == null));

            SynapseController.Server.Map.Doors.Add(new Api.Door(__instance));
        }
    }
}
