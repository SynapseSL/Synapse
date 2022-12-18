using System;

namespace Synapse3.SynapseModule.Dummy;

[Flags]
public enum MovementDirection : byte
{
    None        = 0b_0000,
    Forward     = 0b_0001,
    BackWards   = 0b_0010,
    Right       = 0b_0100,
    Left        = 0b_1000,
}