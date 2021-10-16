using System;
using Assets._Scripts.Dissonance;
using HarmonyLib;
using Synapse.Api.Events.SynapseEventArguments;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    internal static class PlayerSpeakPatch
    {
        [HarmonyPatch(typeof(Radio))]
        internal static class RadioPatch
        {

        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(DissonanceUserSetup.SCPChat), MethodType.Setter)]
        private static bool SCPChatSetter(DissonanceUserSetup __instance, ref bool value)
        {
            var player = __instance.GetPlayer();
            if (player.Team == Team.SCP)
            {
                Synapse.Api.Logger.Get.Debug("VC as SCP");
            }
            return true;
        }

        /*
        private static void InvokeEvent(DissonanceUserSetup __instance)
        {
            try
            {
                var intercom = __instance._dissonanceSetup.IntercomAsHuman;
                var radio = __instance._dissonanceSetup.RadioAsHuman;
                var scp939 = Server.Get.Configs.synapseConfiguration.SpeakingScps.Contains(__instance.GetPlayer().RoleID);
                var scpChat = __instance._dissonanceSetup.SCPChat;
                var specChat = __instance._dissonanceSetup.SpectatorChat;
                var allow = true;

                SynapseController.Server.Events.Player.InvokePlayerSpeakEvent(__instance, ref intercom, ref radio, ref scp939, ref scpChat, ref specChat, ref allow);

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
                        var index = __instance.GetComponent<Radio>().myRadio;

                        if (index != -1 && index < player.VanillaInventory.items.Count)
                        {
                            var item = player.VanillaInventory.items[index].GetSynapseItem();
                            var allowradio = true;
                            SynapseController.Server.Events.Player.InvokePlayerItemUseEvent(player, item, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allowradio);
                            __instance.RadioAsHuman = allowradio;
                        }
                    }
                }
                catch (Exception e)
                {
                    SynapseController.Server.Logger.Error($"Synapse-Event: PlayerUseItemEvent(Radio) failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                }

                return allow;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerSpeak failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    */
    }

    //TODO: Rework
    [HarmonyPatch(typeof(Radio), nameof(Radio.UserCode_CmdSyncTransmissionStatus))]
    internal static class PlayerSpeakPatch1
    {
        [HarmonyPrefix]
        private static bool AltIsActive(Radio __instance, bool b)
        {
            try
            {
                var intercom = __instance._dissonanceSetup.IntercomAsHuman;
                var radio = __instance._dissonanceSetup.RadioAsHuman;
                var scp939 = Server.Get.Configs.synapseConfiguration.SpeakingScps.Contains(__instance.GetPlayer().RoleID);
                var scpChat = __instance._dissonanceSetup.SCPChat;
                var specChat = __instance._dissonanceSetup.SpectatorChat;
                var allow = true;

                SynapseController.Server.Events.Player.InvokePlayerSpeakEvent(__instance._dissonanceSetup, ref intercom, ref radio, ref scp939, ref scpChat, ref specChat, ref allow);

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

                        if (item != null)
                        {
                            var allowradio = true;
                            SynapseController.Server.Events.Player.InvokePlayerItemUseEvent(player, item.GetSynapseItem(), ItemInteractState.Finalizing, ref allowradio);
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