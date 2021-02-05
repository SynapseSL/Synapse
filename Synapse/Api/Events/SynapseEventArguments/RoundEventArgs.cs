using System.Collections.Generic;

namespace Synapse.Api.Events.SynapseEventArguments
{
    public class RoundCheckEventArgs: EventHandler.ISynapseEventArgs
    {        
        public bool EndRound { get; set; }
        
        public RoundSummary.LeadingTeam Team { get; set; }
    }

    public class SpawnPlayersEventArgs: EventHandler.ISynapseEventArgs
    {
        /// <summary>
        /// Determines which Player has which RoleID
        /// </summary>
        public Dictionary<Player,int> SpawnPlayers { get; set; }

        public bool Allow { get; set; }
    }

    public class TeamRespawnEventArgs : EventHandler.ISynapseEventArgs
    {
        private int team;

        public Respawning.SpawnableTeamType Team
        {
            get
            {
                switch (team)
                {
                    case 1: return Respawning.SpawnableTeamType.NineTailedFox;
                    case 2: return Respawning.SpawnableTeamType.ChaosInsurgency;
                    default: return Respawning.SpawnableTeamType.None;
                }
            }
            set
            {
                switch (value)
                {
                    case Respawning.SpawnableTeamType.NineTailedFox:
                        team = 1;
                        break;
                    case Respawning.SpawnableTeamType.ChaosInsurgency:
                        team = 2;
                        break;
                    default:
                        team = -1;
                        break;
                }
            }
        }

        public int TeamID { get => team; set => team = value; }

        public List<Player> Players { get; set; }

        public bool Allow { get; set; } = true;
    }
}