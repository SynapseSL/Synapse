using System;
using HarmonyLib;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(CheaterReport),nameof(CheaterReport.CallCmdReport))]
    internal static class PlayerReportPatch
    {
        private static bool Prefix(CheaterReport __instance, int playerId, string reason, ref bool notifyGm)
        {
            try
            {
                var player = __instance.GetPlayer();
                var target = Server.Get.GetPlayer(playerId);
                if (target == null)
                    return false;
                Server.Get.Events.Player.InvokePlayerReport(player, target, reason, ref notifyGm, out var allow);

                return allow;
            }
            catch(Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerReport failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}
