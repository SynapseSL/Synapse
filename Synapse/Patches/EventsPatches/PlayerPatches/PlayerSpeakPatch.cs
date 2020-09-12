using System;
using Assets._Scripts.Dissonance;
using Harmony;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(DissonanceUserSetup), nameof(DissonanceUserSetup.CallCmdAltIsActive))]
    internal static class PlayerSpeakPatch
    {
        private static bool Prefix(DissonanceUserSetup __instance, bool value)
        {
            try
            {
                var intercom = __instance.IntercomAsHuman;
                var radio = __instance.RadioAsHuman;
                var scp939 = __instance.MimicAs939;
                var scpChat = __instance.SCPChat;
                var specChat = __instance.SpectatorChat;
                var allow = true;
                
                //TODO: Insert Synapse-Speaking SCP's
                
                SynapseController.Server.Events.Player.InvokePlayerSpeakEvent(__instance, ref intercom, ref radio, ref scp939, ref scpChat, ref specChat, ref allow);

                __instance.SCPChat = scpChat;
                __instance.SpectatorChat = specChat;
                __instance.IntercomAsHuman = intercom;

                if (scp939) __instance.MimicAs939 = value;
                if (radio) __instance.RadioAsHuman = value;

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