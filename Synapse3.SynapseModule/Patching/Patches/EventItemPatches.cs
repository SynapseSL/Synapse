using System;
using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Radio;
using InventorySystem.Items.Usables;
using InventorySystem.Items.Usables.Scp330;
using Mirror;
using Neuron.Core.Meta;
using PluginAPI.Enums;
using PluginAPI.Events;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using UnityEngine;
using Utils.Networking;
using StatusMessage = InventorySystem.Items.Usables.StatusMessage;

namespace Synapse3.SynapseModule.Patching.Patches;
#if !PATCHLESS
[Automatic]
[SynapsePatch("ConsumeItem", PatchType.ItemEvent)]
public static class ConsumeItemPatch
{
    private static readonly ItemEvents ItemEvents;
    static ConsumeItemPatch() => ItemEvents = Synapse.Get<ItemEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UsableItemsController), nameof(UsableItemsController.ServerReceivedStatus))]
    public static bool OnReceivedStatus(NetworkConnection conn, StatusMessage msg)
    {
        try
        {
            var player = conn.GetSynapsePlayer();
            if (player == null) return false;
            if (player.Inventory.ItemInHand.Serial != msg.ItemSerial) return false;
            if (player.Inventory.ItemInHand.Item is not UsableItem usableItem) return false;
            var handler = UsableItemsController.GetHandler(player);
            switch (msg.Status)
            {
                case StatusMessage.StatusType.Start:
                    if (!usableItem.ServerValidateStartRequest(handler)) return false;
                    if (handler.CurrentUsable.ItemSerial != 0) return false;
                    if (!usableItem.CanStartUsing) return false;

                    var ev = new ConsumeItemEvent(player.Inventory.ItemInHand, ItemInteractState.Start, player,
                        UsableItemsController.GetCooldown(msg.ItemSerial, usableItem, handler), handler);
                    ev.Allow = ev.RemainingCoolDown <= 0 &&
                               EventManager.ExecuteEvent(ServerEventType.PlayerUseItem, player.Hub, usableItem);
                    ItemEvents.ConsumeItem.RaiseSafely(ev);

                    if (!ev.Allow && ev.RemainingCoolDown > 0f)
                    {
                        conn.Send(new ItemCooldownMessage(msg.ItemSerial, ev.RemainingCoolDown));
                        return false;
                    }

                    if (!ev.Allow) return false;
                    if (usableItem.ItemTypeId.GetSpeedMultiplier(player.Hub) <= 0f) return false;

                    handler.CurrentUsable = new CurrentlyUsedItem(usableItem, msg.ItemSerial, Time.timeSinceLevelLoad);
                    handler.CurrentUsable.Item.OnUsingStarted();
                    new StatusMessage(StatusMessage.StatusType.Start, msg.ItemSerial).SendToAuthenticated();
                    break;

                case StatusMessage.StatusType.Cancel:
                    if (!usableItem.ServerValidateCancelRequest(handler)) return false;
                    if (handler.CurrentUsable.ItemSerial == 0) return false;
                    var modifier = handler.CurrentUsable.Item.ItemTypeId.GetSpeedMultiplier(player.Hub);

                    var remainingCoolDownToCancel = handler.CurrentUsable.StartTime +
                        handler.CurrentUsable.Item.MaxCancellableTime / modifier - Time.timeSinceLevelLoad;

                    ev = new ConsumeItemEvent(player.Inventory.ItemInHand, ItemInteractState.Cancel, player,
                        remainingCoolDownToCancel,
                        handler)
                    {
                        Allow = remainingCoolDownToCancel > 0 && EventManager.ExecuteEvent(
                            ServerEventType.PlayerCancelUsingItem, player.Hub,
                            handler.CurrentUsable.Item)
                    };
                    ItemEvents.ConsumeItem.RaiseSafely(ev);

                    if (ev.Allow)
                    {
                        handler.CurrentUsable.Item.OnUsingCancelled();
                        handler.CurrentUsable = CurrentlyUsedItem.None;
                        new StatusMessage(StatusMessage.StatusType.Cancel, msg.ItemSerial).SendToAuthenticated();
                    }

                    break;
            }

            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Consume Item Patch failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp330NetworkHandler), nameof(Scp330NetworkHandler.ServerSelectMessageReceived))]
    public static bool OnSelectCandy(NetworkConnection conn, SelectScp330Message msg)
    {
        try
        {
            if (msg.Drop) return true;
            var player = conn.GetSynapsePlayer();
            if (player == null) return false;
            if (player.Inventory.ItemInHand.Item is not Scp330Bag bag) return false;
            if (bag.ItemSerial != msg.Serial || msg.CandyID >= bag.Candies.Count || msg.CandyID < 0) return false;

            var ev = new ConsumeItemEvent(player.Inventory.ItemInHand, ItemInteractState.Start, player, 0f,
                UsableItemsController.GetHandler(player));
            ev.CandyID = msg.CandyID;
            ItemEvents.ConsumeItem.RaiseSafely(ev);
            if (!ev.Allow) return false;

            bag.SelectedCandyId = msg.CandyID;
            ev.Handler.CurrentUsable = new CurrentlyUsedItem(bag, msg.Serial, Time.timeSinceLevelLoad);
            ev.Handler.CurrentUsable.Item.OnUsingStarted();
            msg.CandyID = (int)bag.Candies[msg.CandyID];
            msg.SendToAuthenticated();
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Consume Item Patch failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UsableItemsController), nameof(UsableItemsController.Update))]
    public static bool Update()
    {
        try
        {
            if (!StaticUnityMethods.IsPlaying) return false;

            foreach (var handler in UsableItemsController.Handlers)
            {
                handler.Value.DoUpdate(handler.Key);
                var usable = handler.Value.CurrentUsable;
                if (usable.ItemSerial == 0) continue;
                var speed = usable.Item.ItemTypeId.GetSpeedMultiplier(handler.Key);

                //When the player is no longer holding the item
                if (usable.ItemSerial != handler.Key.inventory.CurItem.SerialNumber)
                {
                    if (usable.Item != null)
                        usable.Item.OnUsingCancelled();
                    handler.Value.CurrentUsable = CurrentlyUsedItem.None;
                    handler.Key.inventory.connectionToClient.Send(new StatusMessage(StatusMessage.StatusType.Cancel,
                        usable.ItemSerial));
                    continue;
                }

                if (Time.timeSinceLevelLoad < usable.StartTime + usable.Item.UseTime / speed) continue;

                var allow = EventManager.ExecuteEvent(ServerEventType.PlayerUsedItem, handler.Key, usable.Item);
                var ev = new ConsumeItemEvent(usable.Item.GetItem(), ItemInteractState.Finalize,
                    handler.Key.GetSynapsePlayer(), 0f, handler.Value)
                {
                    Allow = allow
                };
                ItemEvents.ConsumeItem.RaiseSafely(ev);

                if (!ev.Allow)
                {
                    SynapseLogger<Synapse>.Warn("CANCEL FINALIZE EVENT");
                    handler.Value.CurrentUsable.Item.OnUsingCancelled();
                    handler.Value.CurrentUsable = CurrentlyUsedItem.None;
                    new StatusMessage(StatusMessage.StatusType.Cancel, handler.Value.CurrentUsable.ItemSerial)
                        .SendToAuthenticated();
                    ev.Player.Inventory.ItemInHand = SynapseItem.None;
                    continue;
                }

                SynapseLogger<Synapse>.Warn("FINALIZE EVENT");
                usable.Item.ServerOnUsingCompleted();
                Synapse3Extensions.RaiseEvent(typeof(UsableItemsController),
                    nameof(UsableItemsController.ServerOnUsingCompleted), handler.Key, usable.Item);
                handler.Value.CurrentUsable = CurrentlyUsedItem.None;
            }
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Warn("Consume Item Patch failed\n" + ex);
        }

        return false;
    }
}

[Automatic]
[SynapsePatch("RadioInteract", PatchType.ItemEvent)]
public static class RadioInteractPatch
{
    private static readonly ItemEvents ItemEvents;
    static RadioInteractPatch() => ItemEvents = Synapse.Get<ItemEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RadioItem), nameof(RadioItem.ServerProcessCmd))]
    public static bool RadioInteract(RadioItem __instance, RadioMessages.RadioCommand command)
    {
        try
        {
            var item = __instance.GetItem();
            var player = item.ItemOwner;
            var state = (RadioMessages.RadioRangeLevel)__instance._rangeId;
            var nextState = command == RadioMessages.RadioCommand.ChangeRange
                ? ((int)state + 1 >= __instance.Ranges.Length ? 0 : state + 1)
                : state;
            var ev = new RadioUseEvent(item, ItemInteractState.Finalize, player, command, state, nextState);

            switch (ev.RadioCommand)
            {
                case RadioMessages.RadioCommand.Enable:
                    if (__instance._enabled || __instance._battery <= 0f)
                        return false;
                    ev.Allow = EventManager.ExecuteEvent(ServerEventType.PlayerRadioToggle, __instance.Owner,
                        __instance,
                        true);
                    ItemEvents.RadioUse.RaiseSafely(ev);
                    if (!ev.Allow) return false;

                    __instance._enabled = true;
                    break;

                case RadioMessages.RadioCommand.Disable:
                    if (!__instance._enabled) return false;

                    ev.Allow = EventManager.ExecuteEvent(ServerEventType.PlayerRadioToggle, __instance.Owner,
                        __instance,
                        false);
                    ItemEvents.RadioUse.RaiseSafely(ev);
                    if (!ev.Allow) return false;

                    __instance._enabled = false;
                    break;

                case RadioMessages.RadioCommand.ChangeRange:
                    ev.Allow = EventManager.ExecuteEvent(ServerEventType.PlayerChangeRadioRange, __instance.Owner,
                        __instance,
                        (byte)ev.NextRange);
                    ItemEvents.RadioUse.RaiseSafely(ev);
                    if (!ev.Allow) return false;

                    __instance._rangeId = (byte)ev.NextRange;
                    break;
            }

            __instance.SendStatusMessage();
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Warn("Radio Interact Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("MicroExecute", PatchType.ItemEvent)]
public static class MicroExecutePatch
{
    private static readonly ItemEvents ItemEvents;
    static MicroExecutePatch() => ItemEvents = Synapse.Get<ItemEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MicroHIDItem), nameof(MicroHIDItem.ExecuteServerside))]
    public static bool MircoExecute(MicroHIDItem __instance)
    {
        try
        {
            var state = __instance.State;
            var currentEnergy = __instance.EnergyToByte;
            var energyToRemove = 0f;
            var item = __instance.GetItem();
            var ev = new MicroUseEvent(item, state switch
            {
                HidState.Idle => ItemInteractState.Idle,
                HidState.Firing => ItemInteractState.Active,
                HidState.Primed => ItemInteractState.Active,
                HidState.PoweringDown => ItemInteractState.Cancel,
                HidState.PoweringUp => ItemInteractState.Start,
                HidState.StopSound => ItemInteractState.Idle,
                _ => ItemInteractState.Idle
            }, item.ItemOwner, state);
            ItemEvents.MicroUse.RaiseSafely(ev);
            if (!ev.Allow)
            {
                if (__instance.State is not HidState.Idle or HidState.StopSound or HidState.PoweringDown)
                    PowerDown();
                goto END;
            }

            switch (__instance.State)
            {
                case HidState.Idle:
                case HidState.StopSound:
                    if (__instance.RemainingEnergy > 0f && __instance.UserInput != HidUserInput.None)
                    {
                        __instance.State = HidState.PoweringUp;
                        __instance._stopwatch.Restart();
                    }

                    break;

                case HidState.PoweringUp:
                    if ((__instance.UserInput == HidUserInput.None &&
                         __instance._stopwatch.Elapsed.TotalSeconds >= 0.35) || __instance.RemainingEnergy <= 0f)
                        PowerDown();
                    else if (__instance.Readiness == 1f)
                    {
                        __instance.State = __instance.UserInput == HidUserInput.Fire
                            ? HidState.Firing
                            : HidState.Primed;
                        __instance._stopwatch.Restart();
                    }

                    energyToRemove =
                        __instance._energyConsumtionCurve.Evaluate((float)(__instance._stopwatch.Elapsed.TotalSeconds /
                                                                           5.95));
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
                    if ((__instance.UserInput != HidUserInput.Prime &&
                         __instance._stopwatch.Elapsed.TotalSeconds >= 0.35) || __instance.RemainingEnergy <= 0f)
                    {
                        if (__instance.UserInput == HidUserInput.Fire && __instance.RemainingEnergy > 0f)
                        {
                            __instance.State = HidState.Firing;
                            __instance._stopwatch.Restart();
                        }
                        else PowerDown();

                        break;
                    }

                    energyToRemove = __instance._energyConsumtionCurve.Evaluate(1f);
                    break;

                case HidState.Firing:
                    if (__instance._stopwatch.Elapsed.TotalSeconds > 1.7)
                    {
                        energyToRemove = 0.13f;
                        __instance.Fire();
                        if (__instance.RemainingEnergy == 0f || (__instance.UserInput != HidUserInput.Fire &&
                                                                 __instance._stopwatch.Elapsed.TotalSeconds >=
                                                                 2.049999952316284))
                        {
                            if (__instance.RemainingEnergy > 0f && __instance.UserInput == HidUserInput.Prime)
                            {
                                __instance.State = HidState.Primed;
                                __instance._stopwatch.Restart();
                            }
                            else PowerDown();
                        }

                        break;
                    }

                    energyToRemove = __instance._energyConsumtionCurve.Evaluate(1f);
                    break;
            }


            END:

            if (state != __instance.State)
                __instance.ServerSendStatus(HidStatusMessageType.State, (byte)__instance.State);

            if (ev.OverrideEnergyToRemove >= 0f)
                energyToRemove = ev.OverrideEnergyToRemove;
            if (energyToRemove <= 0f) return false;

            __instance.RemainingEnergy = Mathf.Clamp01(__instance.RemainingEnergy - energyToRemove * Time.deltaTime);
            if (currentEnergy != __instance.EnergyToByte)
                __instance.ServerSendStatus(HidStatusMessageType.EnergySync, __instance.EnergyToByte);
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Execute Micro Patch failed\n" + ex);
        }

        return false;

        void PowerDown()
        {
            Synapse3Extensions.RaiseEvent(typeof(MicroHIDItem), nameof(MicroHIDItem.OnStopCharging),
                __instance);
            __instance.State = HidState.PoweringDown;
            __instance._stopwatch.Restart();
        }
    }
}
#endif