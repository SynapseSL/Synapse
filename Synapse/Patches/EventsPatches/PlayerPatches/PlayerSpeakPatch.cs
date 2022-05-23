using System;
using HarmonyLib;
using Synapse.Api.Events.SynapseEventArguments;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    //TODO: Rework ;; okay..
    [HarmonyPatch(typeof(Radio), nameof(Radio.UserCode_CmdSyncTransmissionStatus))]
    internal static class PlayerSpeakPatch
    {
        [HarmonyPrefix]
        private static bool AltIsActive(Radio __instance, bool b)
        {
            try
            {
                var intercom = __instance._dissonanceSetup.IntercomAsHuman;
                var radio = __instance._dissonanceSetup.RadioAsHuman;
                var scp939 = Server.Get.Configs.SynapseConfiguration.SpeakingScps.Contains(__instance.GetPlayer().RoleID);
                var scpChat = __instance._dissonanceSetup.SCPChat;
                var specChat = __instance._dissonanceSetup.SpectatorChat;
                var allow = true;

                SynapseController.Server.Events.Player.InvokePlayerSpeakEvent(__instance._dissonanceSetup, ref intercom,
                    ref radio, ref scp939, ref scpChat, ref specChat, ref allow);

                __instance._dissonanceSetup.SCPChat = scpChat;
                __instance._dissonanceSetup.SpectatorChat = specChat;
                __instance._dissonanceSetup.IntercomAsHuman = intercom;

                if (scp939) __instance._dissonanceSetup.MimicAs939 = b;
                else __instance._dissonanceSetup.MimicAs939 = false;

                if (radio) __instance._dissonanceSetup.RadioAsHuman = b;

                try
                {
                    if (__instance._dissonanceSetup.RadioAsHuman)
                    {
                        var player = __instance.GetPlayer();
                        var item = __instance.RadioItem;

                        if (item is not null)
                        {
                            var allowradio = true;
                            SynapseController.Server.Events.Player.InvokePlayerItemUseEvent(player, item.GetSynapseItem(),
                                ItemInteractState.Finalizing, ref allowradio);
                            __instance._dissonanceSetup.RadioAsHuman = allowradio;
                        }
                    }
                }
                catch (Exception e)
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