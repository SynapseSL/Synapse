using System;
using GameCore;
using HarmonyLib;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), typeof(GameObject), typeof(long), typeof(string),
        typeof(string), typeof(bool))]
    internal static class PlayerBanPatch
    {
        [HarmonyPrefix]
        private static bool BanUser(GameObject user, long duration, string reason, string issuer, bool isGlobalBan)
        {
            try
            {
                var player = user.GetPlayer();
                Api.Player banIssuer;
                if (issuer.Contains("("))
                {
                    banIssuer = SynapseController.Server.GetPlayer(issuer.Substring(issuer.LastIndexOf('(') + 1, issuer.Length - 2 - issuer.LastIndexOf('(')));
                }
                else
                {
                    banIssuer = SynapseController.Server.GetPlayer(issuer);
                }
                var allow = true;
                SynapseController.Server.Events.Player.InvokePlayerBanEvent(player, banIssuer, ref duration, ref reason, ref allow);

                return isGlobalBan && ConfigFile.ServerConfig.GetBool("gban_ban_ip") || allow;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerBan failed!!\n{e}");
                return true;
            }
        }
    }
}