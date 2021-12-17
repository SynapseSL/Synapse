using System;
using HarmonyLib;
using Mirror;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(CustomNetworkManager), nameof(CustomNetworkManager.OnServerDisconnect))]
    internal static class PlayerLeavePatch
    {
        [HarmonyPrefix]
        private static void OnDisconnect(NetworkConnection conn)
        {
            try
            {
                if (conn == null || conn.identity == null) return;

                var player = conn.identity.GetPlayer();
                if (player == null) return;

                if (player.CustomRole != null)
                    player.CustomRole = null;

                SynapseController.Server.Events.Player.InvokePlayerLeaveEvent(player);
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerLeave failed!!\n{e}");
            }
        }
    }
}