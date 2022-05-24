using HarmonyLib;
using System;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(TantrumEnvironmentalHazard), nameof(TantrumEnvironmentalHazard.DistanceChanged))]
    internal static class PlayerWalkOnTantrumPatch
    {
        [HarmonyPrefix]
        private static bool DistanceChanged(TantrumEnvironmentalHazard __instance, ReferenceHub player)
        {
            try
            {
                if (player is null || __instance.DisableEffect || __instance._correctPosition is null)
                    return false;

                var synapseplayer = player.GetPlayer();
                
                if (Vector3.Distance(player.transform.position, __instance._correctPosition.position) > __instance.DistanceToBeAffected)
                    return false;
                
                var allow = true;

                if ((__instance.SCPImmune && synapseplayer.Team == (int)Team.SCP) || !SynapseExtensions.CanHarmScp(synapseplayer, false) || synapseplayer.GodMode)
                    allow = false;
               
                Synapse.Api.Events.EventHandler.Get.Player.InvokeTantrum(synapseplayer, __instance, ref allow);

                if (allow)
                {
                    Synapse.Api.Logger.Get.Debug("ALLOW TANTRUM");
                    synapseplayer.PlayerEffectsController.EnableEffect<CustomPlayerEffects.Stained>(2f, false);
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Synapse.Api.Logger.Get.Error("Synapse-Event: PlayerWalkOnSinkholeEvent failed!!\n" + ex);
                return true;
            }
        }
    }
}
