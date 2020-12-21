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
                var player = __instance.GetPlayer();
                SynapseController.Server.Events.Player.InvokePlayerLeaveEvent(player);
                if (player.CustomRole != null)
                    player.CustomRole = null;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerLeave failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }
        }
    }
}