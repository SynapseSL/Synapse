using Swan;
using Synapse.Network;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "NetworkClients",
        Aliases = new[] {"netclients"},
        Description = "Lists all connected clients",
        Usage = "netclients <id>",
        Permission = "synapse.command.networkclients",
        Platforms = new[] {Platform.RemoteAdmin, Platform.ServerConsole}
    )]
    public class SynapseNetworkClientsCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if (context.Arguments.Count == 0)
            {
                var awaiter = SynapseNetworkClient.GetClient.Details().GetAwaiter();
                awaiter.OnCompleted(() =>
                {
                    context.Player.SendRAConsoleMessage("Response: \n" + awaiter.GetResult().Humanize());
                });
            }
            else
            {
                var id = context.Arguments.At(0);
                var awaiter = SynapseNetworkClient.GetClient.Details(id).GetAwaiter();
                awaiter.OnCompleted(() =>
                {
                    context.Player.SendRAConsoleMessage("Response: \n" + awaiter.GetResult().Humanize());
                });
            }

            result.Message = "Getting Clients...";
            return result;
        }
    }
}