namespace Synapse.Api.Events.SynapseEventArguments
{
    public class RoundCheckEventArgs: EventHandler.ISynapseEventArgs
    {
        public bool ForceEnd { get; set; }
        
        public bool Allow { get; set; }
        
        public RoundSummary.LeadingTeam Team { get; set; }
        
        public bool TeamChanged { get; set; }
    }
}