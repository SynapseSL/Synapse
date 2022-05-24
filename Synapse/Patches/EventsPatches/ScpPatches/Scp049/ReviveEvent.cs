using HarmonyLib;
using System;
using UnityEngine;
using static Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp049
{
    [HarmonyPatch(typeof(PlayableScps.Scp049), nameof(PlayableScps.Scp049.BodyCmd_ByteAndGameObject))]
    internal static class ReviveEvent
    {
        [HarmonyPrefix]
        private static bool OnInteract(PlayableScps.Scp049 __instance, byte num, GameObject go)
        {
            try
            {
                if (num != 1 && num != 2)
                    return true;

                var scp = __instance.GetPlayer();
                var ragdoll = go.GetComponent<Ragdoll>().GetRagdoll();
                var target = ragdoll.Owner;

                Get.Scp.Scp049.InvokeRevive(scp, target, ragdoll, num == 2, out var allow);
                return allow;
            }
            catch (Exception ex)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp049ReviveEvent failed!!\n{ex}");
                return true;
            }
        }
    }
}
