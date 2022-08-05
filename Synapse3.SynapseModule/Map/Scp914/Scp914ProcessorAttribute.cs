using System;

namespace Synapse3.SynapseModule.Map.Scp914;

public class Scp914ProcessorAttribute : Attribute
{
    public uint[] ReplaceHandlers { get; set; }
}