﻿using System;

namespace Synapse3.SynapseModule.Map.Scp914;

public class Scp914ProcessorAttribute : Attribute
{
    public int[] ReplaceHandlers { get; set; }
}