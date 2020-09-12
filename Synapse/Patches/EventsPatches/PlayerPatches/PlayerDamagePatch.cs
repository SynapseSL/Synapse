using System;
using Harmony;
using Synapse.Api;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.HurtPlayer))]
    internal static class PlayerDamagePatch
    {
        private static void Prefix(PlayerStats __instance, ref PlayerStats.HitInfo info, GameObject go)
        {
            try
            {
                if (go == null) return;
                
                var killer = __instance.GetPlayer();

                if (info.GetDamageType() == DamageTypes.Grenade)
                    killer = SynapseController.Server.GetPlayer(info.PlayerId);

                var player = go.GetPlayer();
                
                //TODO: Implement Player Hurt Event

                if (player.GodMode) return;
                
                if(player.Health + player.ArtificialHealth - info.Amount <= 0)
                    SynapseController.Server.Events.Player.InvokePlayerDeathEvent(player, killer, info);
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerDamage failed!!\n{e}");
            }
        }
    }
}