using System;
using HarmonyLib;
using Synapse.Api;
using EventHandler = Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp106
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdContain106))]
    internal class Scp106ContainmentPatch
    {
        private static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                var allow = true;
                EventHandler.Get.Scp.Scp106.InvokeScp106ContainmentEvent(__instance.GetPlayer(), ref allow);
                return allow;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: Scp106Containment failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}