using System;
using HarmonyLib;
using Synapse.Api;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp079
{
    [HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.RpcSwitchCamera))]
    internal static class Scp079CameraSwitchPatch
    {
        [HarmonyPrefix]
        private static bool SwitchCamera(Scp079PlayerScript __instance, ushort camId, bool lookatRotation)
        {
            try
            {
                var camera = Server.Get.Map.Cameras.Find(cam => cam.ID == camId);
                Player player = __instance.GetPlayer();
                bool spawning = false;
                if (!player.Scp079Controller.Spawned)
                {
                    spawning = true;
                    player.Scp079Controller.Spawned = true;
                }

                SynapseController.Server.Events.Scp.Scp079.Invoke079CameraSwitch(
                    __instance.gameObject.GetPlayer(),
                    camera,
                    lookatRotation,
                    spawning,
                    out var allowed
                    );

                return allowed;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp079CameraSwitchEvent failed!!\n{e}");
                return true;
            }
        }
    }
}
