using System;
using HarmonyLib;

namespace Synapse.Patches.EventsPatches.RoundPatches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.RpcRoundStarted))]
    internal static class RoundStartPatch
    {
        private static void Prefix()
        {
            try
            {
                SynapseController.Server.Events.Round.InvokeRoundStartEvent();
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: RoundStartEvent failed!!\n{e}");
            }
        }
    }
}