using System;
using Harmony;
using Synapse.Api;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.LoadComponents))]
    internal static class ComponentsPatch
    {
        private static void Prefix(ReferenceHub __instance)
        {
            if (__instance.GetComponent<Player>() == null) 
                __instance.gameObject.AddComponent<Player>();

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