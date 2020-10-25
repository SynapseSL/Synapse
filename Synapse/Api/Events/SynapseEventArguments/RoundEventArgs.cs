using System.Collections.Generic;

namespace Synapse.Api.Events.SynapseEventArguments
{
    public class RoundCheckEventArgs: EventHandler.ISynapseEventArgs
    {
        public bool ForceEnd { get; set; }
        
        public bool Allow { get; set; }
        
        public RoundSummary.LeadingTeam Team { get; set; }
        
        public bool TeamChanged { get; set; }
    }

    public class SpawnPlayersEventArgs: EventHandler.ISynapseEventArgs
    {
        /// <summary>
        /// This determined which Player gets which RoleID
        /// </summary>
        public Dictionary<Player,int> SpawnPlayers { get; set; }

        public bool Allow { get; set; }
    }
}