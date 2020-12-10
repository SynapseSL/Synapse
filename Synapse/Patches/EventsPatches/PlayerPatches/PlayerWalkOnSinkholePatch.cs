using System;
using UnityEngine;
using HarmonyLib;
using Mirror;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(SinkholeEnvironmentalHazard),nameof(SinkholeEnvironmentalHazard.DistanceChanged))]
    internal static class PlayerWalkOnSinkholePatch
    {
        private static bool Prefix(SinkholeEnvironmentalHazard __instance, GameObject player)
        {
            try
            {
                if (!NetworkServer.active) return false;

                var component = player?.GetComponentInParent<PlayerEffectsController>();
                if (component == null) return false;

                var sinkholeeffect = component.GetEffect<CustomPlayerEffects.SinkHole>();
                var synapseplayer = player.GetPlayer();

                if(Vector3.Distance(player.transform.position,__instance.transform.position) <= __instance.DistanceToBeAffected)
                {
                    var allow = true;
                    if (__instance.SCPImmune && synapseplayer.RealTeam == Team.SCP || synapseplayer.GodMode)
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
