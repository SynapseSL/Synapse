using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using Synapse.Api;
using Synapse.Config;
using Synapse.Database;

// ReSharper disable All
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

                Task.Run(() =>
                {
                    if (!SynapseController.Server.Configs.synapseConfiguration.DatabaseEnabled) return;
                    if (!DatabaseManager.PlayerRepository.ExistGameId(player.UserId))
                    {
                        var dbo = new PlayerDbo()
                        {
                            GameIdentifier = player.UserId,
                            Name = player.DisplayName,
                            DoNotTrack = player.DoNotTrack,
                            Data = new Dictionary<string, string>()
                        };
                        DatabaseManager.PlayerRepository.Insert(dbo);
                    }
                    else
                    {
                        var dbo = DatabaseManager.PlayerRepository.FindByGameId(player.UserId);
                        dbo.Name = player.DisplayName;
                        dbo.DoNotTrack = player.DoNotTrack;
                        DatabaseManager.PlayerRepository.Save(dbo);
                    }
                });
                
                if(!string.IsNullOrEmpty(player.UserId))
                {
                   SynapseController.Server.Events.Player.InvokePlayerJoinEvent(player, ref n);
                }
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerJoin failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }
        }
    }
}