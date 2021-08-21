using System;
using HarmonyLib;
using InventorySystem.Items.MicroHID;
using UnityEngine;
using ItemState = Synapse.Api.Events.SynapseEventArguments.ItemInteractState;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(MicroHIDItem),nameof(MicroHIDItem.ExecuteServerside))]
    internal static class PlayerUseMicroPatch
    {
        private static bool Prefix(MicroHIDItem __instance)
        {
            try
            {
                var state = __instance.State;
                var energyToByte = __instance.EnergyToByte;
                var num = 0f;
                var allow = true;
                var owner = __instance.Owner.GetPlayer();
                var item = __instance.GetSynapseItem();

                var itemstate = ItemState.Initiating;
                switch (state)
                {
                    case HidState.Firing: itemstate = ItemState.Finalizing; break;
                    case HidState.Idle: itemstate = ItemState.Stopping; break;
                    case HidState.PoweringDown: itemstate = ItemState.Stopping; break;
                    case HidState.PoweringUp: itemstate = ItemState.Initiating; break;
                    case HidState.Primed: itemstate = ItemState.Finalizing; break;
                }

                Server.Get.Events.Player.InvokePlayerItemUseEvent(owner,item, itemstate, ref allow);
                if (!allow)
                    state = HidState.Idle;

                Server.Get.Events.Player.InvokeMicroUse(owner, item, ref state);

                switch (state)
                {
                    case HidState.Idle:
                        if (__instance.RemainingEnergy > 0f && __instance.UserInput != HidUserInput.None)
                        {
                            __instance.State = HidState.PoweringUp;
                            __instance._stopwatch.Restart();
                        }
                        break;

                    case HidState.PoweringUp:
                        if ((__instance.UserInput == HidUserInput.None && __instance._stopwatch.Elapsed.TotalSeconds >= 0.35) || __instance.RemainingEnergy <= 0f)
                        {
                            __instance.State = HidState.PoweringDown;
                            __instance._stopwatch.Restart();
                        }
                        else if (__instance._stopwatch.Elapsed.TotalSeconds >= 5.95)
                        {
                            __instance.State = ((__instance.UserInput == HidUserInput.Fire) ? HidState.Firing : HidState.Primed);
                            __instance._stopwatch.Restart();
                        }

                        num = __instance._energyConsumtionCurve.Evaluate((float)(__instance._stopwatch.Elapsed.TotalSeconds / 5.95));
                        break;

                    case HidState.PoweringDown:
                        if (__instance._stopwatch.Elapsed.TotalSeconds >= 3.1)
                        {
                            __instance.State = HidState.Idle;
                            __instance._stopwatch.Stop();
                            __instance._stopwatch.Reset();
                        }
                        break;

                    case HidState.Primed:
                        if ((__instance.UserInput != HidUserInput.Prime && __instance._stopwatch.Elapsed.TotalSeconds >= 0.34999999403953552) || __instance.RemainingEnergy <= 0f)
                        {
                            __instance.State = ((__instance.UserInput == HidUserInput.Fire && __instance.RemainingEnergy > 0f) ? HidState.Firing : HidState.PoweringDown);
                            __instance._stopwatch.Restart();
                        }
                        else
                        {
                            num = __instance._energyConsumtionCurve.Evaluate(1f);
                        }
                        break;

                    case HidState.Firing:
                        if (__instance._stopwatch.Elapsed.TotalSeconds > 1.7000000476837158)
                        {
                            num = 0.13f;
                            __instance.Fire();
                            if (__instance.RemainingEnergy == 0f || (__instance.UserInput != HidUserInput.Fire && __instance._stopwatch.Elapsed.TotalSeconds >= 2.05))
                            {
                                __instance.State = ((__instance.RemainingEnergy > 0f && __instance.UserInput == HidUserInput.Prime) ? HidState.Primed : HidState.PoweringDown);
                                __instance._stopwatch.Restart();
                            }
                        }
                        else
                        {
                            num = __instance._energyConsumtionCurve.Evaluate(1f);
                        }
                        break;
                }

                if (state != __instance.State)
                {
                    __instance.ServerSendStatus(HidStatusMessageType.State, (byte)__instance.State);
                }

                if (num > 0f)
                {
                    __instance.RemainingEnergy = Mathf.Clamp01(__instance.RemainingEnergy - num * Time.deltaTime);
                    if (energyToByte != __instance.EnergyToByte)
                    {
                        __instance.ServerSendStatus(HidStatusMessageType.EnergySync, __instance.EnergyToByte);
                    }
                }

                return false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerUseMicro failed!!\n{e}");
                return true;
            }
        }
    }
}
