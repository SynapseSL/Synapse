using HarmonyLib;
using Synapse.Api;
using System;
using static Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    [HarmonyPatch(typeof(PlayerInteract),nameof(PlayerInteract.UserCode_CmdUsePanel))]
    internal static class WarheadPanelInteract
    {
        [HarmonyPrefix]
        private static bool OnPanelInteract(PlayerInteract __instance ,PlayerInteract.AlphaPanelOperations n)
        {
            try
            {
                if (Map.Get.Nuke.InsidePanel.Locked) return false;

                if (!__instance.CanInteract || !__instance.ChckDis(AlphaWarheadOutsitePanel.nukeside.transform.position)) return false;
                var player = __instance.GetPlayer();

                Get.Map.InvokeWarheadPanel(player, n == PlayerInteract.AlphaPanelOperations.Cancel, out var allow);

                return allow;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: WarheadPanelInteract Event failed!!\n{e}");
                return false;
            }
        }
    }
}
