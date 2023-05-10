using System;

namespace Synapse3.SynapseModule.Map.Elevators;

[AttributeUsage(AttributeTargets.Class)]
public class ElevatorAttribute : Attribute
{
    public string Name { get; set; }
    public uint Id { get; set; }
    public uint ChamberSchematicId { get; set; }
    public uint DestinationSchematicId { get; set; }
}