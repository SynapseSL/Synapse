using System;
using Harmony;
using UnityEngine;

// ReSharper disable All
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

                if (player.GodMode) return;

                SynapseController.Server.Events.Player.InvokePlayerDamageEvent(player, killer, ref info);
                
                if(player.Health + player.ArtificialHealth - info.Amount <= 0)
                {
                    SynapseController.Server.Events.Player.InvokePlayerDeathEvent(player, killer, info);
                    if (player.CustomRole != null)
                        player.CustomRole = null;
                }
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerDamage failed!!\n{e}");
            }
        }
    }
}