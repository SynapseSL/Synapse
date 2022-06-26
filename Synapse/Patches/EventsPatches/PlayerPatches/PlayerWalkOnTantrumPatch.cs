using HarmonyLib;
using System;
using Synapse.Api;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(TantrumEnvironmentalHazard), nameof(TantrumEnvironmentalHazard.OnEnter))]
    internal static class PlayerWalkOnTantrumPatch
    {
        [HarmonyPrefix]
        private static bool OnEnter(TantrumEnvironmentalHazard __instance, ReferenceHub player)
        {
            try
            {
                var sPlayer = (Player)player;
                bool allow = !(!SynapseExtensions.CanHarmScp(sPlayer, false) || sPlayer.GodMode);
                
                Synapse.Api.Events.EventHandler.Get.Player.InvokeTantrum(sPlayer, __instance, ref allow);

                return allow;
            }
            catch (Exception ex)
            {
                Api.Logger.Get.Error("Synapse-Event: PlayerWalkOnSinkholeEvent failed!!\n" + ex);
                return true;
            }
        }
    }
}
