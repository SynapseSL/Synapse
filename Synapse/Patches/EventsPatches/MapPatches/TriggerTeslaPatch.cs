using System;
using HarmonyLib;
using System.Collections.Generic;
using Synapse.Api;
using System.Linq;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.PlayersInRange))]
    internal static class TriggerTeslaPatch
    {
        private static void Postfix(TeslaGate __instance, bool hurtRange, ref List<PlayerStats> __result)
        {
            try
            {
                __result = new List<PlayerStats>();
                var Tesla = Map.Get.Teslas.FirstOrDefault(x => x.GameObject == __instance.gameObject);

                foreach(var player in SynapseController.Server.Players)
                {
                    if (Vector3.Distance(Tesla.Position, player.Position) > Tesla.SizeOfTrigger || player.IsDead) 
                        continue;

                    if (player.Invisible)
                        continue;

                    SynapseController.Server.Events.Map.InvokeTriggerTeslaEv(player, Tesla, hurtRange, out var trigger);

                    if (trigger) __result.Add(player.PlayerStats);
                }
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: TriggerTesla failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }
        }
    }
}
