﻿namespace Synapse3.SynapseModule.Command;

/// <summary>
/// A Enum that represents the Current Command Platform
/// </summary>
public enum CommandPlatform
{
    /// <summary>
    /// The Platform that is only used by the ServerConsole
    /// </summary>
    ServerConsole,
    
    /// <summary>
    /// The Platform that can be used by all Players
    /// </summary>
    PlayerConsole,
    
    /// <summary>
    /// The Platform that can be used by all Admins with access to the RemoteAdmin
    /// </summary>
    RemoteAdmin
}