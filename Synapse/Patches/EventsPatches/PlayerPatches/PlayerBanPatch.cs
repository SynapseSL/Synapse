﻿using System;
using GameCore;
using HarmonyLib;
using Synapse.Api;
using UnityEngine;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), typeof(GameObject), typeof(int), typeof(string), typeof(string), typeof(bool))]
    internal static class PlayerBanPatch
    {
        private static bool Prefix(GameObject user, int duration, string reason, string issuer, bool isGlobalBan)
        {
            try
            {
                
                var player = user.GetPlayer();
                var banIssuer = SynapseController.Server.GetPlayer(issuer);
                var allow = true;
                SynapseController.Server.Events.Player.InvokePlayerBanEvent(player, banIssuer, ref duration, ref reason, ref allow);

                return isGlobalBan && ConfigFile.ServerConfig.GetBool("gban_ban_ip") || allow;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerBan failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}