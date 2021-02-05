namespace Synapse.Api.Teams
{
    public abstract class SynapseTeam : ISynapseTeam
    {
        public SynapseTeamInformation Info { get; set; }

        public abstract void Spawn();

        public virtual void Initialise() { }
    }
}
