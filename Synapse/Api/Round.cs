﻿using GameCore;
using Respawning;
using System;
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

        public void MtfRespawn(bool isCI = false) 
            => RespawnManager.Singleton.ForceSpawnTeam(isCI ? SpawnableTeamType.ChaosInsurgency : SpawnableTeamType.NineTailedFox);
    }
}
