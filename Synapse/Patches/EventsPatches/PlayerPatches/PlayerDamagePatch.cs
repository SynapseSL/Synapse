using System;
using System.Linq;
using HarmonyLib;
using Synapse.Api;
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
                var player = go.GetPlayer();

                if (player == null) return;

                if (info.GetDamageType() == DamageTypes.Grenade)
                    killer = SynapseController.Server.GetPlayer(info.PlayerId);

                if (info.GetDamageType() == DamageTypes.Pocket)
                    killer = Server.Get.Players.FirstOrDefault(x => x.Scp106Controller.PocketPlayers.Contains(player));

                if (player.GodMode) return;

                SynapseController.Server.Events.Player.InvokePlayerDamageEvent(player, killer, ref info);
                
                if(player.Health + player.ArtificialHealth - info.Amount <= 0)
                {
                    SynapseController.Server.Events.Player.InvokePlayerDeathEvent(player, killer, info);

                    foreach (var ply in Server.Get.GetPlayers(x => x.Scp106Controller.PocketPlayers.Contains(player)))
                        ply.Scp106Controller.PocketPlayers.Remove(player);

                    player.CustomRole = null;
                }
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerDamage failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }
        }
    }
}