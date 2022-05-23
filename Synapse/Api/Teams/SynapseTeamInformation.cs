using System;
namespace Synapse.Api.Teams
{
    public class SynapseTeamInformation : Attribute
    {
        public int ID { get; set; }

        public string Name { get; set; }
    }
}