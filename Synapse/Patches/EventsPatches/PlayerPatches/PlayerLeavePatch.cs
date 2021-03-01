using System;
using HarmonyLib;
using Mirror;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(CustomNetworkManager), nameof(CustomNetworkManager.OnServerDisconnect))]
    internal static class PlayerLeavePatch
    {
        private static void Prefix(NetworkConnection conn)
        {
            try
            {
                var player = conn?.identity?.GetPlayer();
                if (player == null) return;

                if (player.CustomRole != null)
                    player.CustomRole = null;
                SynapseController.Server.Events.Player.InvokePlayerLeaveEvent(player);
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerLeave failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }
        }
    }
}