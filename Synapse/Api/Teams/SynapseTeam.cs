using System.Collections.Generic;

namespace Synapse.Api.Teams
{
    public abstract class SynapseTeam : ISynapseTeam
    {
        public SynapseTeamInformation Info { get; set; }

        public abstract void Spawn(List<Player> players);

        public virtual void Initialise() { }
    }
}
