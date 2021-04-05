using System;
using GameCore;
using HarmonyLib;
using Swan;
using Synapse.Database;
using UnityEngine;
using Logger = Synapse.Api.Logger;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), typeof(GameObject), typeof(int), typeof(string),
        typeof(string), typeof(bool))]
    internal static class PlayerBanPatch
    {
        private static bool Prefix(GameObject user, int duration, string reason, string issuer, bool isGlobalBan)
        {
            try
            {
                var player = user.GetPlayer();
                var banIssuer = SynapseController.Server.GetPlayer(issuer);
                var allow = true;
                SynapseController.Server.Events.Player.InvokePlayerBanEvent(player, banIssuer, ref duration, ref reason,
                    ref allow);
                var finalizedAllow = isGlobalBan && ConfigFile.ServerConfig.GetBool("gban_ban_ip") || allow;
                if (finalizedAllow && Server.Get.Configs.synapseConfiguration.DatabaseBans)
                {
                    var time = DateTime.Now;
                    var dbo = new PunishmentDbo
                    {
                        Receiver = player.UserId,
                        Issuer = banIssuer.UserId,
                        Type = PunishmentType.Ban,
                        Timestamp = time.ToUnixEpochDate(),
                        Expire = time.AddSeconds(duration).ToUnixEpochDate(),
                        Message = reason ?? "",
                        Note = ""
                    };
                    DatabaseManager.PunishmentRepository.Insert(dbo);
                    dbo.Kick(player);
                    Logger.Get.Info("Ban has been issued");
                    return false;
                }

                return finalizedAllow;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error(
                    $"Synapse-Event: PlayerBan failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}