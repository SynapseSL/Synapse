using System;
using HarmonyLib;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.LoadComponents))]
    internal static class ComponentsPatch
    {
        [HarmonyPrefix]
        private static void LoadComponents(ReferenceHub __instance)
        {
            try
            {
                SynapseController.Server.Events.Player.InvokeLoadComponentsEvent(__instance.gameObject);
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: LoadComponents failed!!\n{e}");
            }
        }
    }
}