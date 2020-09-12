using System;
using Harmony;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.UpdateNickname))]
    internal static class PlayerJoinPatch
    {
        private static void Prefix(NicknameSync __instance, ref string n)
        {
            try
            {
                var player = __instance.GetPlayer();
                
                if(!string.IsNullOrEmpty(player.UserId))
                    SynapseController.Server.Events.Player.InvokePlayerJoinEvent(player, ref n);
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerJoin failed!!\n{e}");
            }
        }
    }
}