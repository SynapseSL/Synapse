﻿using Neuron.Modules.Commands;

namespace Synapse3.SynapseModule.CommandService.SynapseCommands;

[SynapseCommand(
    CommandName = "Test",
    Aliases = new []{ "te" },
    Description = "Command for testing purposes",
    Permission = "synapse.test",
    Platforms = new[] { CommandPlatform.PlayerConsole, CommandPlatform.RemoteAdmin , CommandPlatform.ServerConsole }
    )]
public class TestCommand : SynapseCommand
{
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        result.Response = "Test message";
    }
}