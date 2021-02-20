using System;
using HarmonyLib;
using UnityEngine;
using PlayableScps;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp096
{
    [HarmonyPatch(typeof(PlayableScps.Scp096), nameof(PlayableScps.Scp096.UpdateVision))]
    internal static class Scp096LookPatch
    {
        private static bool Prefix(PlayableScps.Scp096 __instance)
        {
            try
            {
                if (__instance._flash.Enabled)
                {
                    return false;
                }
                var vector = __instance.Hub.transform.TransformPoint(PlayableScps.Scp096._headOffset);
                foreach (var player in Server.Get.Players)
                {
                    var characterClassManager = player.ClassManager;
                    if (characterClassManager.CurClass != RoleType.Spectator && !(player.Hub == __instance.Hub) && !characterClassManager.IsAnyScp() && Vector3.Dot((player.CameraReference.position - vector).normalized, __instance.Hub.PlayerCameraReference.forward) >= 0.1f)
                    {
                        var visionInformation = VisionInformation.GetVisionInformation(player.Hub, vector, -0.1f, 60f, true, true, __instance.Hub.localCurrentRoomEffects);
                        if (visionInformation.IsLooking)
                        {
                            float delay = visionInformation.LookingAmount / 0.25f * (visionInformation.Distance * 0.1f);
                            if (!__instance.Calming)
                            {
                                if (player.Invisible || Server.Get.Configs.synapseConfiguration.CantRage096.Contains(player.RoleID))
                                    continue;

                                if (player.RealTeam == Team.SCP && !Server.Get.Configs.synapseConfiguration.ScpTrigger096)
                                    continue;

                                Server.Get.Events.Scp.Scp096.InvokeScpTargetEvent(player, __instance.GetPlayer(), __instance.PlayerState, out var allow);
                                __instance.AddTarget(player.gameObject);
                                if (!allow) continue;
                            }
                            if (__instance.CanEnrage && player.gameObject != null)
                            {
                                __instance.PreWindup(delay);
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp096AddTargetEvent failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayableScps.Scp096), nameof(PlayableScps.Scp096.OnDamage))]
    internal static class Scp096ShootPatch
    {
        private static bool Prefix(PlayableScps.Scp096 __instance, PlayerStats.HitInfo info)
        {
            try
            {
                if (info == null || info.RHub == null) return false;

                var player = info.RHub.GetPlayer();

                if (player.Invisible || Server.Get.Configs.synapseConfiguration.CantRage096.Contains(player.RoleID))
                    return false;

                if (player.RealTeam == Team.SCP && !Server.Get.Configs.synapseConfiguration.ScpTrigger096)
                    return false;

                Server.Get.Events.Scp.Scp096.InvokeScpTargetEvent(player, __instance.GetPlayer(), __instance.PlayerState, out var allow);
                return allow;
            }
            catch (Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp096AddTargetEvent failed!!\n{e}");
                return true;
            }
        }
    }
}