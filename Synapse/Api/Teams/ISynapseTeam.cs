using System.Collections.Generic;

namespace Synapse.Api.Teams
{
    public interface ISynapseTeam
    {
        SynapseTeamInformation Info { get; set; }

        void Spawn(List<Player> players);

        void Initialise();
    }
}
