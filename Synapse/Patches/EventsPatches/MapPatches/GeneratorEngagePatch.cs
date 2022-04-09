using System;
using HarmonyLib;
using MapGeneration.Distributors;
using Logger = Synapse.Api.Logger;

namespace Synapse.Events.Patches
{
    [HarmonyPatch(typeof(Scp079Generator), nameof(Scp079Generator.ServerSetFlag))]
	internal static class GeneratorEngagePatch
    {
		[HarmonyPrefix]
		private static bool ServerSetFlag(Scp079Generator __instance, Scp079Generator.GeneratorFlags flag)
		{
            try
            {
                if (flag != Scp079Generator.GeneratorFlags.Engaged) return true;
                bool allow = true;
                Server.Get.Events.Map.InvokeGeneratorEngage(__instance, ref allow);
                return allow;
            }
            catch (Exception e)
            {
				Logger.Get.Error($"Synapse-Event: GeneratorEngage event failed!!\n{e}");
                return true;
			}
        }
    }
}