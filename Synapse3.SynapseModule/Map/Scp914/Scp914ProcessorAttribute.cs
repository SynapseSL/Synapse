using System;

namespace Synapse3.SynapseModule.Map.Scp914;

[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class Scp914ProcessorAttribute : Attribute
{
    public uint[] ReplaceHandlers { get; set; }
}