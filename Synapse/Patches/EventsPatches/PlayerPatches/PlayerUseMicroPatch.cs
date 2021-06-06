using System;
using HarmonyLib;
using Mirror;
using UnityEngine;
using Logger = Synapse.Api.Logger;
using ItemState = Synapse.Api.Events.SynapseEventArguments.ItemInteractState;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(MicroHID),nameof(MicroHID.UpdateServerside))]
    internal static class PlayerUseMicroPatch
    {
        private static bool Prefix(MicroHID __instance)
        {
            try
            {
                if (!NetworkServer.active)
                    return false;

                if (__instance.refHub.inventory.curItem == ItemType.MicroHID)
                {
                    // The GetEnergy Method can throw a Error when GetItemIndex returns -1.
                    //This can happen when equiping a micro and drop it so that curItem and itemuniq are not synced for a frame or something like this
                    try
                    {
                        if (__instance.GetEnergy() != __instance.Energy)
                            __instance.ChangeEnergy(__instance.Energy);
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                    foreach (var item in __instance.refHub.inventory.items)
                        if (item.id == ItemType.MicroHID)
                            __instance.NetworkEnergy = item.durability;

                if (__instance.keyAntiSpamCooldown > 0f) __instance.keyAntiSpamCooldown -= Time.deltaTime;

                if(__instance.refHub.inventory.curItem == ItemType.MicroHID || __instance.chargeup > 0f)
                {
                    if(__instance.CurrentHidState != MicroHID.MicroHidState.Idle)
                    {
                        __instance.refHub.weaponManager.scp268.ServerDisable();
                        __instance._visionController.MakeNoise(__instance.CurrentHidState == MicroHID.MicroHidState.Discharge ? 20 : 75);
                    }

                    MicroHID.MicroHidState state = MicroHID.MicroHidState.Idle;

                    if (__instance.refHub.inventory.curItem == ItemType.MicroHID)
                    {
                        if (__instance.Energy > 0f && __instance.chargeup >= 1f && __instance.SyncKeyCode == 2)
                            state = MicroHID.MicroHidState.Discharge;
                        else if (__instance.Energy > 0f && __instance.chargeup < 1f && __instance.SyncKeyCode != 0 && __instance.CurrentHidState != MicroHID.MicroHidState.RampDown)
                            state = MicroHID.MicroHidState.RampUp;
                        else if (__instance.chargeup > 0f && (__instance.SyncKeyCode == 0 || __instance.Energy <= 0f || __instance.CurrentHidState == MicroHID.MicroHidState.RampDown))
                            state = MicroHID.MicroHidState.RampDown;
                        else if (__instance.chargeup <= 0f && (__instance.SyncKeyCode == 0 || __instance.Energy <= 0f || __instance.CurrentHidState == MicroHID.MicroHidState.RampDown))
                            state = MicroHID.MicroHidState.Idle;
                        else if (__instance.chargeup >= 1f)
                            state = MicroHID.MicroHidState.Spinning;
                    }
                    else state = MicroHID.MicroHidState.RampDown;

                    var player = __instance.refHub.GetPlayer();
                    var item = player.ItemInHand;

                    SynapseController.Server.Events.Player.InvokeMicroUse(player, item, ref state);
                    //ItemUseEvent Invoke
                    if(state != MicroHID.MicroHidState.Idle)
                    {
                        var itemstate = ItemState.Initiating;
                        switch (state)
                        {
                            case MicroHID.MicroHidState.Spinning:
                            case MicroHID.MicroHidState.Discharge:
                                itemstate = ItemState.Finalizing;
                                break;

                            case MicroHID.MicroHidState.RampUp:
                                itemstate = ItemState.Initiating;
                                break;

                            case MicroHID.MicroHidState.RampDown:
                                itemstate = ItemState.Stopping;
                                break;
                        }
                        var allow = true;
                        if (item != null)
                            SynapseController.Server.Events.Player.InvokePlayerItemUseEvent(player, item, itemstate, ref allow);
                        if (!allow) state = MicroHID.MicroHidState.Idle;
                    }

                    switch (state)
                    {
                        case MicroHID.MicroHidState.Discharge:
                            if (__instance.soundEffectPause >= 1f)
                            {
                                __instance.NetworkEnergy = Mathf.Clamp01(__instance.Energy - Time.deltaTime * __instance.dischargeEnergyLoss);
                                __instance.DealDamage();
                            }
                            else __instance.NetworkEnergy = Mathf.Clamp01(__instance.Energy - Time.deltaTime * __instance.speedBasedEnergyLoss.Evaluate(1f));
                            break;

                        case MicroHID.MicroHidState.RampUp:
                            __instance.chargeup = Mathf.Clamp01(__instance.chargeup + Time.deltaTime / __instance.chargeupTime);
                            __instance.NetworkEnergy = Mathf.Clamp01(__instance.Energy - Time.deltaTime * __instance.speedBasedEnergyLoss.Evaluate(__instance.chargeup));
                            break;

                        case MicroHID.MicroHidState.RampDown:
                            __instance.chargeup = Mathf.Clamp01(__instance.chargeup - Time.deltaTime / __instance.chargedownTime);
                            break;

                        case MicroHID.MicroHidState.Spinning:
                            __instance.NetworkEnergy = Mathf.Clamp01(__instance.Energy - Time.deltaTime * __instance.speedBasedEnergyLoss.Evaluate(__instance.chargeup));
                            break;

                        //Idle does nothing
                    }

                    __instance.NetworkCurrentHidState = state;
                }

                if (__instance.Energy <= 0.05f) __instance.NetworkEnergy = 0f;

                return false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerUseMicro failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}
