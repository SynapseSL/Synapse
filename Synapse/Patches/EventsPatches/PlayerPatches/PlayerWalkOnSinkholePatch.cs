using System;
using HarmonyLib;
using Synapse.Api;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(SinkholeEnvironmentalHazard),nameof(SinkholeEnvironmentalHazard.OnEnter))]
    internal static class PlayerWalkOnSinkholePatch
    {
        [HarmonyPrefix]
        private static bool OnEnter(SinkholeEnvironmentalHazard __instance, ReferenceHub player)
        {
            try
            {
                var sPlayer = (Player)player;
                bool allow = !(!SynapseExtensions.CanHarmScp(sPlayer, false) || sPlayer.GodMode);
                
                Synapse.Api.Events.EventHandler.Get.Player.InvokeSinkhole(sPlayer, __instance, ref allow);

                return allow;
            }
            catch (Exception ex)
            {
                Logger.Get.Error($"Synapse-Event: PlayerWalkOnSinkholeEvent failed!!\n{ex}");
                return true;
            }
        }
    }
}
