using HarmonyLib;
using MapGeneration.Distributors;
using System;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    [HarmonyPatch(typeof(Scp079Generator), nameof(Scp079Generator.ServerUpdate))]
    internal static class GeneratorEngagePatch
    {
        [HarmonyPrefix]
        private static bool OnUpdate(Scp079Generator __instance)
        {
            try
            {
				var flag = __instance._currentTime >= __instance._totalActivationTime;

				if (!flag)
				{
					int num = Mathf.FloorToInt(__instance._totalActivationTime - __instance._currentTime);
					if (num != __instance._syncTime)
						__instance.Network_syncTime = (short)num;
				}

				if (__instance.ActivationReady)
				{
					if (flag && !__instance.Engaged)
					{
						Synapse.Api.Events.EventHandler.Get.Map.InvokeGenEngage(__instance.GetGenerator(), out var allow);

						if (!allow) return false;

						__instance.Engaged = true;
						__instance.Activating = false;
						return false;
					}
					__instance._currentTime += Time.deltaTime;
				}
				else
				{
					if (__instance._currentTime == 0f || flag)
						return false;

					__instance._currentTime -= __instance.DropdownSpeed * Time.deltaTime;
				}

				__instance._currentTime = Mathf.Clamp(__instance._currentTime, 0f, __instance._totalActivationTime);
				return false;
			}
            catch (Exception ex)
            {
                Logger.Get.Error($"Synapse-Event: GeneratorEngaged Patch failed!!\n{ex}");
                return true;
            }
        }
    }
}
