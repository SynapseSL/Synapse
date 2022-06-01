using HarmonyLib;
using Synapse.Api;
using System;
using static Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.UserCode_CmdDetonateWarhead))]
    internal static class StartWarheadPatch
    {
        [HarmonyPrefix]
        private static bool OnCmdDetonate(PlayerInteract __instance)
        {
            try
            {
                if (!__instance.CanInteract || !Nuke.Get.OutsidePanel.KeyCardEntered || !Nuke.Get.InsidePanel.Enabled)
                    return false;
                var player = __instance.GetPlayer();

                Get.Map.InvokeWarheadStart(player, out var allow);

                return allow;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: WarheadStart Event failed!!\n{e}");
                return true;
            }
        }
    }
}
