using HarmonyLib;
using Synapse.Api;
using Synapse.Api.Enum;
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
                if (player == (ReferenceHub)null || __instance.DisableEffect || (UnityEngine.Object)__instance._correctPosition == (UnityEngine.Object)null)
                    return false;
                var synapseplayer = player.GetPlayer();
                var effectsController = player.playerEffectsController;
                
                if ((double)Vector3.Distance(player.transform.position, __instance._correctPosition.position) > (double)__instance.DistanceToBeAffected)
                    return false;
                
                var allow = true;
                if (__instance.SCPImmune && !SynapseExtensions.CanHarmScp(synapseplayer, false) || synapseplayer.GodMode)
                    allow = false;
               
                Synapse.Api.Events.EventHandler.Get.Player.InvokeTantrum(synapseplayer, __instance, ref allow);
                
                if (allow)
                    synapseplayer.GiveEffect(Effect.Stained, (byte)0, 2f);
                
                return false;
            }
            catch (Exception ex)
            {
                Synapse.Api.Logger.Get.Error(string.Format("Synapse-Event: PlayerWalkOnSinkholeEzvent failed!!\n{0}", (object)ex));
                return true;
            }
        }
    }
}
