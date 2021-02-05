namespace Synapse.Api.Teams
{
    public interface ISynapseTeam
    {
        SynapseTeamInformation Info { get; set; }

        void Spawn();

        void Initialise();
    }
}
