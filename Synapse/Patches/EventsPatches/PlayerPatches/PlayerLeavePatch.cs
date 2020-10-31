using System;
using HarmonyLib;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.OnDestroy))]
    internal static class PlayerLeavePatch
    {
        private static void Prefix(ReferenceHub __instance)
        {
            try
            {
                SynapseController.Server.Events.Player.InvokePlayerLeaveEvent(__instance.GetPlayer());
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerLeave failed!!\n{e}");
            }
        }
    }
}