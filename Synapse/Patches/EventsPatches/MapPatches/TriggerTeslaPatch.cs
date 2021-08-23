using System;
using HarmonyLib;
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

                __result = __instance.InRange(synapseplayer.Position);

                if (__result)
                    Server.Get.Events.Map.InvokeTriggerTeslaEv(synapseplayer, __instance.GetTesla(), ref __result);

                return false;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: TriggerTesla failed!!\n{e}");
                return true;
            }
        }
    }
}
