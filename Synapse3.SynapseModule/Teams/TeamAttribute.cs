using System;

namespace Synapse3.SynapseModule.Teams;

public class TeamAttribute : Attribute
{
    public uint Id { get; set; }
    
    public string Name { get; set; }
}