using System;
using Assets._Scripts.Dissonance;
using HarmonyLib;
using Synapse.Api.Events.SynapseEventArguments;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    // Setters are also being called when the player is - for instance - not an SCP, yet it sets SCPChat to "false" for no reason
    // Setting it to

    [HarmonyPatch(typeof(DissonanceUserSetup), nameof(DissonanceUserSetup.SCPChat), MethodType.Setter)]
    internal static class PlayerSpeakPatch
    {
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
        */

        [HarmonyPrefix]
        [HarmonyPatch(nameof(DissonanceUserSetup.RadioAsHuman), MethodType.Setter)]
        private static bool RadioAsHumanSetter(DissonanceUserSetup __instance, ref bool value)
        {
            var player = __instance.GetPlayer();
            if (player.Team != Team.SCP && player.Team != Team.RIP) //Human = not scp & not spectator
            {
                Synapse.Api.Logger.Get.Debug("VC as Human");
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(DissonanceUserSetup.SpectatorChat), MethodType.Setter)]
        private static bool SpectatorChatSetter(DissonanceUserSetup __instance, ref bool value)
        {
            Synapse.Api.Logger.Get.Debug(value);
            var player = __instance.GetPlayer();
            if (player.Team == Team.RIP)
            {
                Synapse.Api.Logger.Get.Debug("VC as Human");
            }
            return true;
        }
        
        /*
        private static void InvokeEvent(DissonanceUserSetup __instance)
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
}