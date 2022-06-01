﻿using HarmonyLib;
using Synapse.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.UpdateNickname))]
    internal static class PlayerJoinPatch
    {
        [HarmonyPrefix]
        private static void UpdateNickname(NicknameSync __instance, ref string n)
        {
            try
            {
                var player = __instance.GetPlayer();

                _ = Task.Run(() =>
                {
                    if (!SynapseController.Server.Configs.SynapseConfiguration.DatabaseEnabled)
                        return;
                    if (!DatabaseManager.PlayerRepository.ExistGameId(player.UserId))
                    {
                        var dbo = new PlayerDbo()
                        {
                            GameIdentifier = player.UserId,
                            Name = player.DisplayName,
                            DoNotTrack = player.DoNotTrack,
                            Data = new Dictionary<string, string>()
                        };
                        _ = DatabaseManager.PlayerRepository.Insert(dbo);
                    }
                    else
                    {
                        var dbo = DatabaseManager.PlayerRepository.FindByGameId(player.UserId);
                        dbo.Name = player.DisplayName;
                        dbo.DoNotTrack = player.DoNotTrack;
                        _ = DatabaseManager.PlayerRepository.Save(dbo);
                    }
                });

                if (!String.IsNullOrEmpty(player.UserId))
                {
                    SynapseController.Server.Events.Player.InvokePlayerJoinEvent(player, ref n);
                }
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerJoin failed!!\n{e}");
            }
        }
    }
}