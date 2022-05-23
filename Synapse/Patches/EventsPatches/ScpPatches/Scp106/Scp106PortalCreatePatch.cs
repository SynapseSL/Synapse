﻿using System;
using HarmonyLib;
using Synapse.Api;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp106
{
    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.UserCode_CmdMakePortal))]
    internal static class Scp106PortalCreatePatch
    {
        [HarmonyPrefix]
        private static bool CreatePortal(Scp106PlayerScript __instance)
        {
            try
            {
                Server.Get.Events.Scp.Scp106.InvokePortalCreateEvent(__instance.GetPlayer(), out var allow);
                return allow;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: Scp106PortalCreate failed!!\n{e}");
                return true;
            }
        }
    }
}