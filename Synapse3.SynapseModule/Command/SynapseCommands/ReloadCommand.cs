using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseRaCommand(
    CommandName = "Reload",
    Aliases = new[] { "rl" },
    Parameters = new string[] { },
    Description = "Reloads Synapse",
    Permission = "synapse.reload",
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
        catch
        {
            result.StatusCode = CommandStatusCode.Error;
            result.Response = "Couldn't reload Synapse.See Server console for more Information";
        }
    }
}