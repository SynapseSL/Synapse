using HarmonyLib;
using InventorySystem.Items.Radio;
using Synapse.Api;
using System;
using static Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(RadioItem), nameof(RadioItem.ServerProcessCmd))]
    internal static class PlayerRadioInteractPatch
    {
        [HarmonyPrefix]
        private static bool OnProcessCmd(RadioMessages.RadioCommand command, RadioItem __instance)
        {
            try
            {
                var item = __instance.GetSynapseItem();
                var player = item.ItemHolder;
                var state = (RadioMessages.RadioRangeLevel)__instance._radio.NetworkcurRangeId;
                var nextstate = (int)state + 1 >= __instance.Ranges.Length ? 0 : state + 1;

                Get.Player.InvokeRadio(player, item, ref command, state, ref nextstate, out var allow);
                if (!allow) return false;

                switch (command)
                {
                    case RadioMessages.RadioCommand.Enable:
                        __instance._enabled = true;
                        break;

                    case RadioMessages.RadioCommand.Disable:
                        __instance._enabled = false;
                        __instance._radio.ForceDisableRadio();
                        break;

                    case RadioMessages.RadioCommand.ChangeRange:
                        __instance._rangeId = (byte)nextstate;
                        __instance._radio.NetworkcurRangeId = __instance._rangeId;
                        break;
                }

                __instance.SendStatusMessage();

                return false;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: RadioInteractEvent failed!!\n{e}");
                return true;
            }
        }
    }
}