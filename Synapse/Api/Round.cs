using GameCore;
using Respawning;
using Respawning.NamingRules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Synapse.Api
{
    public class Round
    {
        public static Round Get => Map.Get.Round;

        internal Round() { }

        private RoundSummary Rs => RoundSummary.singleton;
        private RespawnManager Rm => RespawnManager.Singleton;

        public int CurrentRound { get; internal set; } = 0;

        public float NextRespawn
        {
            get => Rm._timeForNextSequence - (Rm._stopwatch.Elapsed.Hours * 3600 + Rm._stopwatch.Elapsed.Minutes * 60 + Rm._stopwatch.Elapsed.Seconds);
            set => Rm._timeForNextSequence = value + (Rm._stopwatch.Elapsed.Hours * 3600 + Rm._stopwatch.Elapsed.Minutes * 60 + Rm._stopwatch.Elapsed.Seconds);
        }

        public bool PrioritySpawn
        {
            get => Rm._prioritySpawn;
            set => Rm._prioritySpawn = value;
        }

        public bool LobbyLock
        {
            get => RoundStart.LobbyLock;
            set => RoundStart.LobbyLock = value;
        }

        public bool RoundLock
        {
            get => RoundSummary.RoundLock;
            set => RoundSummary.RoundLock = value;
        }

        public int EscapedDPersonnel
        {
            get => RoundSummary.escaped_ds;
            set => RoundSummary.escaped_ds = value;
        }

        public int EscapedScientists
        {
            get => RoundSummary.escaped_scientists;
            set => RoundSummary.escaped_scientists = value;
        }

        public int ScpKills
        {
            get => RoundSummary.kills_by_scp;
            set => RoundSummary.kills_by_scp = value;
        }

        public int MtfTickets
        {
            get => RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox);
            set => RespawnTickets.Singleton._tickets[SpawnableTeamType.NineTailedFox] = value;
        }

        public int ChaosTickets
        {
            get => RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.ChaosInsurgency);
            set => RespawnTickets.Singleton._tickets[SpawnableTeamType.ChaosInsurgency] = value;
        }

        internal bool Forceend { get; set; } = false;

        public TimeSpan RoundLength => RoundStart.RoundLength;

        public bool RoundIsActive => RoundSummary.RoundInProgress();

        public bool RoundEnded => Rs.RoundEnded;

        public void StartRound() => CharacterClassManager.ForceRoundStart();

        public void EndRound() => Forceend = true;

        public void RestartRound() => Server.Get.Host.PlayerStats.Roundrestart();

        public void SpawnVehicle(bool IsCI = false) => RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.Selection, IsCI ? SpawnableTeamType.ChaosInsurgency : SpawnableTeamType.NineTailedFox);

        public void PlayChaosSpawnSound() => RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.UponRespawn, SpawnableTeamType.ChaosInsurgency);

        public void DimScreens() => Rs.RpcDimScreen();

        public void ShowRoundSummary(RoundSummary.SumInfo_ClassList remainingPlayers,RoundSummary.LeadingTeam team)
        {
            var timeToRoundRestart = Mathf.Clamp(GameCore.ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);

            Rs.RpcShowRoundSummary(Rs.classlistStart,remainingPlayers, team, EscapedDPersonnel, EscapedScientists, ScpKills, timeToRoundRestart);
        }

        public void MtfRespawn(bool isCI = false) => MtfRespawn(isCI, RoleType.Spectator.GetPlayers(), true);

        public void MtfRespawn(bool isCI, List<Player> players, bool useTicket = true)
        {
            if (!players.Any()) return;
            SpawnableTeamType Team = isCI ? SpawnableTeamType.ChaosInsurgency : SpawnableTeamType.NineTailedFox;

            players.RemoveAll(p => p.OverWatch);
            if (!players.Any()) return;
            Queue<RoleType> queueToFill = new Queue<RoleType>();
            SpawnableTeamHandlerBase spawnableTeamHandlerBase = RespawnWaveGenerator.SpawnableTeams[Team];
            spawnableTeamHandlerBase.GenerateQueue(queueToFill, players.Count);

            if (useTicket)
            {
                if (PrioritySpawn)
                    players = players.OrderBy(p => p.DeathTime).ToList();
                else
                    players.ShuffleList();

                int tickets = RespawnTickets.Singleton.GetAvailableTickets(Team);
                if (tickets == 0)
                {
                    tickets = 5;
                    ChaosTickets = 5;
                }
                int num = Mathf.Min(tickets, spawnableTeamHandlerBase.MaxWaveSize);

                players.RemoveRange(players.Count - num, num);
            }
            players.ShuffleList();

            string unityName = "";
            bool setUnite = UnitNamingRules.TryGetNamingRule(Team, out UnitNamingRule rule);

            if (setUnite)
            {
                rule.GenerateNew(Team, out unityName);
                rule.PlayEntranceAnnouncement(unityName);
            }

            foreach (var player in players)
            {
                try
                {
                    if (player == null)
                    {
                        Logger.Get.Error("Couldn't spawn a player - target's is null.");
                        continue;
                    }

                    RoleType role = queueToFill.Dequeue();
                    player.ClassManager.SetPlayersClass(role, player.gameObject, CharacterClassManager.SpawnReason.Respawn);

                    if (setUnite)
                    {
                        player.ClassManager.NetworkCurSpawnableTeamType = (byte)Team;
                        player.UnitName = unityName;
                    }
                }
                catch (Exception e)
                {
                    Logger.Get.Error($"Player {player.name} couldn't be spawned. Err msg: {e.Message}");
                }
            }
            RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.UponRespawn, Team);
            RespawnManager.Singleton.RestartSequence();
        }
    }
}
