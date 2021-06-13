using System;
using HarmonyLib;
using Synapse.Api;

namespace Synapse.Patches.EventsPatches.RoundPatches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.RpcRoundStarted))]
    internal static class RoundStartPatch
    {
        private static void Prefix()
        {
            try
            {
                Logger.Get.Warn("Round Start !");
                SynapseController.Server.Events.Round.InvokeRoundStartEvent();
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: RoundStartEvent failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }
        }
    }
}