using System;
using Harmony;
using Synapse.Api;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp106
{
    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdMakePortal))]
    internal static class Scp106PortalCreatePatch
    {
        private static bool Prefix(Scp106PlayerScript __instance)
        {
            try
            {
                Server.Get.Events.Scp.Scp106.InvokePortalCreateEvent(__instance.GetPlayer(), out var allow);
                return allow;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: Scp106PortalCreate failed!!\n{e}");
                return true;
            }
        }
    }
}
