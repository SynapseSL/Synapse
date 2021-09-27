﻿using System;
using Assets._Scripts.Dissonance;
using HarmonyLib;
using Synapse.Api.Events.SynapseEventArguments;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    //TODO:
    //[HarmonyPatch(typeof(DissonanceUserSetup), nameof(DissonanceUserSetup.UserCode_CmdAltIsActive))]
    internal static class PlayerSpeakPatch
    {
        [HarmonyPrefix]
        private static bool AltIsActive(DissonanceUserSetup __instance, bool value)
        {
            try
            {
                var intercom = __instance.IntercomAsHuman;
                var radio = __instance.RadioAsHuman;
                var scp939 = Server.Get.Configs.synapseConfiguration.SpeakingScps.Contains(__instance.GetPlayer().RoleID);
                var scpChat = __instance.SCPChat;
                var specChat = __instance.SpectatorChat;
                var allow = true;
                
                SynapseController.Server.Events.Player.InvokePlayerSpeakEvent(__instance, ref intercom, ref radio, ref scp939, ref scpChat, ref specChat, ref allow);

                __instance.SCPChat = scpChat;
                __instance.SpectatorChat = specChat;
                __instance.IntercomAsHuman = intercom;

                if (scp939) __instance.MimicAs939 = value;
                else __instance.MimicAs939 = false;

                if (radio) __instance.RadioAsHuman = value;

                try
                {
                    if (__instance.RadioAsHuman)
                    {
                        var player = __instance.GetPlayer();
                        var item = __instance.GetComponent<Radio>().RadioItem;

                        if (item != null)
                        {
                            var allowradio = true;
                            SynapseController.Server.Events.Player.InvokePlayerItemUseEvent(player, item.GetSynapseItem(), ItemInteractState.Finalizing, ref allowradio);
                            __instance.RadioAsHuman = allowradio;
                        }
                    }
                }
                catch(Exception e)
                {
                    SynapseController.Server.Logger.Error($"Synapse-Event: PlayerUseItemEvent(Radio) failed!!\n{e}");
                }

                return allow;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerSpeak failed!!\n{e}");
                return true;
            }
        }
    }
}