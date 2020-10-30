using GameCore;
using Respawning;
using System;
using UnityEngine;

namespace Synapse.Api
{
    public class Round
    {
        internal Round() { }

        private RoundSummary rs => RoundSummary.singleton;

        public int CurrentRound { get; internal set; } = 0;

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

        internal bool Forceend { get; set; } = false;

        public TimeSpan RoundLength => RoundStart.RoundLenght;

        public bool RoundIsActive => RoundSummary.RoundInProgress();

        public bool RoundEnded => rs._roundEnded;

        public void StartRound() => CharacterClassManager.ForceRoundStart();

        public void EndRound() => Forceend = true;

        public void RestartRound() => Server.Get.Host.PlayerStats.Roundrestart();

        public void DimScreens() => rs.RpcDimScreen();

        public void ShowRoundSummary(RoundSummary.SumInfo_ClassList remainingPlayers,RoundSummary.LeadingTeam team)
        {
            var timeToRoundRestart = Mathf.Clamp(GameCore.ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);

            rs.RpcShowRoundSummary(rs.classlistStart,remainingPlayers, team, EscapedDPersonnel, EscapedScientists, ScpKills, timeToRoundRestart);
        }

        public void MtfRespawn(bool isCI = false)
        {
            var component = Server.Get.Host.GetComponent<RespawnManager>();
            component.NextKnownTeam = isCI ? SpawnableTeamType.ChaosInsurgency : SpawnableTeamType.NineTailedFox;
            component.Spawn();
        }
    }
}
