using System;

namespace Synapse3.SynapseModule.Teams;

public class UnitAttribute : Attribute
{
    public byte Id { get; set; }

    public uint[] DefaultRolesInUnit { get; set; }
    
    public uint AssignedTeam { get; set; }
}