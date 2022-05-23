using HarmonyLib;
using Synapse.Api.Events.SynapseEventArguments;
using System;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    [HarmonyPatch(typeof(Lift), nameof(Lift.MovePlayers))]
    internal class LiftMoveObjectsPatch
    {
        [HarmonyPrefix]
        private static bool OnMovePlayers(Lift __instance, ref LiftMoveObjectsEventArgs __state, ref Transform target)
        {
            try
            {
                __state = new()
                {
                    Elevator = __instance.GetElevator(),
                    Transform = target,
                };
                Server.Get.Events.Map.InvokeLiftMoveObjects(__state);
                if (!__state.Allow) return false;
                target = __state.Transform;
                return true;
            }
            catch (Exception ex)
            {
                Api.Logger.Get.Error($"Synapse-Event: LiftMovePlayers Event failed!!\n{ex}");
                return true;
            }
        }

        [HarmonyPostfix]
        private static void EndMovePlayers(LiftMoveObjectsEventArgs __state)
        {
            if (__state.DeleteTransform)
                GameObject.Destroy(__state.Transform);
        }
    }
}