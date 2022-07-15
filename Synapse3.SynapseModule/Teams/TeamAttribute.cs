using System;

namespace Synapse3.SynapseModule.Teams;

public class TeamAttribute : Attribute
{
    public int Id { get; set; }
    
    public string Name { get; set; }
}