using GameCore;
using Respawning;
using System;

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

        public TimeSpan RoundLength => RoundStart.RoundLenght;

        public bool RoundIsActive => RoundSummary.RoundInProgress();

        public bool RoundEnded => rs._roundEnded;

        public void StartRound() => CharacterClassManager.ForceRoundStart();

        public void EndRound() => rs.ForceEnd();

        public void RestartRound() => Server.Get.Host.PlayerStats.Roundrestart();

        public void DimScreens() => rs.RpcDimScreen();

        public void ShowRoundSummary(RoundSummary.SumInfo_ClassList list_start, RoundSummary.SumInfo_ClassList list_finish,
            RoundSummary.LeadingTeam leadingTeam, int e_ds, int e_sc, int scp_kills, int round_cd)
                => rs.RpcShowRoundSummary(list_start, list_finish, leadingTeam, e_ds, e_sc, scp_kills, round_cd);

        public void MtfRespawn(bool isCI = false)
        {
            var component = Server.Get.Host.GetComponent<RespawnManager>();
            component.NextKnownTeam = isCI ? SpawnableTeamType.ChaosInsurgency : SpawnableTeamType.NineTailedFox;
            component.Spawn();
        }
    }
}
