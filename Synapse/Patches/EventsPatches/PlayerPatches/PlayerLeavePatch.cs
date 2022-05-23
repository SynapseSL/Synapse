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
                if (conn is null || conn.identity is null) return;

                var player = conn.identity.GetPlayer();
                if (player is null) return;

                if (player.CustomRole is not null)
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