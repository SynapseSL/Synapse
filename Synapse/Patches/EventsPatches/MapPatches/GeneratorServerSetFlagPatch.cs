using System;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using MapGeneration.Distributors;
using Logger = Synapse.Api.Logger;

namespace Synapse.Events.Patches
{
    [HarmonyPatch(typeof(Scp079Generator), nameof(Scp079Generator.ServerSetFlag))]
	internal static class GeneratorServerSetFlagPatch
    {
		[HarmonyPrefix]
		private static bool ServerSetFlag(Scp079Generator __instance, Scp079Generator.GeneratorFlags flag, bool state)
		{
            try
            {
                Server.Get.Events.Map.InvokeGeneratorGeneratorServerSetFlag(__instance, flag, state);
            }
            catch (Exception e)
            {
				Logger.Get.Error($"Synapse-Event: GeneratorEngage event failed!!\n{e}");
			}
            return true;
        }
    }
}