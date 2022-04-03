using System;
using HarmonyLib;
using MapGeneration.Distributors;
using Logger = Synapse.Api.Logger;

namespace Synapse.Events.Patches
{
    [HarmonyPatch(typeof(Scp079Generator), nameof(Scp079Generator.ServerSetFlag))]
	internal static class GeneratorEngagedPatch
    {
		[HarmonyPrefix]
		private static bool OnEngage(Scp079Generator __instance, Scp079Generator.GeneratorFlags flag, bool state)
		{
            try
            {
                if (flag != Scp079Generator.GeneratorFlags.Engaged) return true;

                Server.Get.Events.Map.InvokeGeneratorEngage(__instance, ref state);
                return state;
            }
            catch (Exception e)
            {
				Logger.Get.Error($"Synapse-Event: GeneratorEngage event failed!!\n{e}");
				return true;
			}
        }
    }
}