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
        public Respawning.SpawnableTeamType Team { get; set; }

        public List<Player> Players { get; set; }

        public bool Allow { get; set; } = true;
    }
}