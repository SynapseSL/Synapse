using System;
using HarmonyLib;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.PlayerInRange))]
    internal static class TriggerTeslaPatch
    {
        private static bool Prefix(TeslaGate __instance, out bool __result, ReferenceHub player)
        {
            __result = false;
            try
            {
                var synapseplayer = player.GetPlayer();
                if (synapseplayer.Invisible) return false;

                __result = Vector3.Distance(__instance.transform.position, player.playerMovementSync.RealModelPosition) < __instance.sizeOfTrigger;

                if (__result)
                    Server.Get.Events.Map.InvokeTriggerTeslaEv(synapseplayer, __instance.GetTesla(), ref __result);

                return false;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: TriggerTesla failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}
