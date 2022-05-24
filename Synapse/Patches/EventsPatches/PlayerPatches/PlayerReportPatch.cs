using System;
using HarmonyLib;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(CheaterReport),nameof(CheaterReport.UserCode_CmdReport))]
    internal static class PlayerReportPatch
    {
        [HarmonyPrefix]
        private static bool Report(CheaterReport __instance, int playerId, string reason, ref bool notifyGm)
        {
            try
            {
                var player = __instance.GetPlayer();
                var target = Server.Get.GetPlayer(playerId);
                if (target is null)
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
