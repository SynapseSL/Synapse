using System;
using HarmonyLib;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(SinkholeEnvironmentalHazard),nameof(SinkholeEnvironmentalHazard.DistanceChanged))]
    internal static class PlayerWalkOnSinkholePatch
    {
        [HarmonyPrefix]
        private static bool DistanceChanged(SinkholeEnvironmentalHazard __instance, ReferenceHub player)
        {
            try
            {
                var synapseplayer = player.GetPlayer();

                if(Vector3.Distance(synapseplayer.Position ,__instance.transform.position) <= __instance.DistanceToBeAffected)
                {
                    var allow = true;
                    if (__instance.SCPImmune && !SynapseExtensions.CanHarmScp(synapseplayer, false) || synapseplayer.GodMode) 
                        allow = false;

                    Synapse.Api.Events.EventHandler.Get.Player.InvokeSinkhole(synapseplayer, __instance, ref allow);

                    if (allow)
                    {
                        synapseplayer.GiveEffect(Api.Enum.Effect.SinkHole);
                        return false;
                    }
                }

                synapseplayer.GiveEffect(Api.Enum.Effect.SinkHole, 0);

                return false;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerWalkOnSinkholeEvent failed!!\n{e}");
                return true;
            }
        }
    }
}
