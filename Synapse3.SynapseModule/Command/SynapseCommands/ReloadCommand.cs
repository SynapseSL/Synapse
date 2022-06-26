using System;
using Neuron.Core.Logging;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseRaCommand(
    CommandName = "Reload",
    Aliases = new[] { "rl" },
    Parameters = new string[] { },
    Description = "Reloads Synapse",
    Permission = "synapse.command.reload",
    Platforms = new [] { CommandPlatform.RemoteAdmin, CommandPlatform.ServerConsole }
)]
public class ReloadCommand : SynapseCommand
{
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        try
        {
            Synapse.Get<ServerService>().Reload();
            result.StatusCode = CommandStatusCode.Ok;
            result.Response = "Synapse Reloaded";
        }
        catch (Exception ex)
        {
            result.StatusCode = CommandStatusCode.Error;
            result.Response = "Couldn't reload Synapse.See Server console for more Information";
            NeuronLogger.For<Synapse>().Error($"Sy3 Command: reload command failed\n{ex}");
        }
    }
}