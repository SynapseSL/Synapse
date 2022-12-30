using System;

namespace Synapse3.SynapseModule.Teams;

[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class TeamAttribute : Attribute
{
    public uint Id { get; set; }
    
    public string Name { get; set; }
}