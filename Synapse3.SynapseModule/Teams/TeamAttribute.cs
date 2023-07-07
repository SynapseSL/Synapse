using System;

namespace Synapse3.SynapseModule.Teams;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TeamAttribute : Attribute
{
    public uint Id { get; set; }
    
    public string Name { get; set; }
    
    public bool EvacuatePlayers { get; set; }
}