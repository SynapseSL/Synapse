using System;

namespace Synapse3.SynapseModule.Map.Scp914;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class Scp914ProcessorAttribute : Attribute
{
    public uint[] ReplaceHandlers { get; set; }
}