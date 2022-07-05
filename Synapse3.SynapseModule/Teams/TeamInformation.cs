using System;

namespace Synapse3.SynapseModule.Teams;

public class TeamInformation : Attribute
{
    public int Id { get; set; }
    
    public string Name { get; set; }
}