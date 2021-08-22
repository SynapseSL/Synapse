using System;
using HarmonyLib;
using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp079
{
	[HarmonyPatch(typeof(Recontainer079), nameof(Recontainer079.EndOvercharge))]
	internal class EndOverchargePatch
    {
		private static void Postfix()
		{
			try
			{
				SynapseController.Server.Events.Scp.Scp079.Invoke079RecontainEvent(Recontain079Status.Finished, out var allow);
			}
			catch (Exception e)
			{
				Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp079RecontainEvent Finished failed!!\n{e}");
			}
		}
	}

	[HarmonyPatch(typeof(Recontainer079), nameof(Recontainer079.RefreshActivator))]
	internal static class Scp079RecontainPatch
	{
		[HarmonyPrefix]
		private static bool OnActivator(Recontainer079 __instance)
        {
            try
            {
				if (__instance._delayStopwatch.Elapsed.TotalSeconds > __instance._activationDelay)
				{
					if (!__instance._delayStopwatch.IsRunning)
						return false;

					Server.Get.Events.Scp.Scp079.Invoke079RecontainEvent(Recontain079Status.Start, out var allow);

                    if (!allow)
                    {
						__instance._alreadyRecontained = false;
						__instance._delayStopwatch.Stop();
						__instance._delayStopwatch.Reset();
						return false;
                    }

					__instance.BeginOvercharge();
					__instance._delayStopwatch.Stop();
					__instance._unlockStopwatch.Start();
					return false;
				}
				else
				{
					if (!__instance._activatorGlass.isBroken)
						return false;

					__instance._activatorButton.transform.localPosition = Vector3.Lerp(__instance._activatorButton.transform.localPosition, __instance._activatorPos, __instance._activatorLerpSpeed * Time.deltaTime);
					
					if (__instance._alreadyRecontained)
						return false;

					if (__instance.CassieBusy)
						return false;

					Server.Get.Events.Scp.Scp079.Invoke079RecontainEvent(Recontain079Status.Initialize, out var allow);
					if (!allow) return false;

					__instance.Recontain();
					return false;
				}
			}
			catch(Exception e)
            {
				Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp079RecontainEvent Activator failed!!\n{e}");
				return true;
            }
        }
	}
}
