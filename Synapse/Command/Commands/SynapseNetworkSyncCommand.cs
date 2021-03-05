using System.Linq;
using Swan;
using Synapse.Network;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "NetworkSync",
        Aliases = new[] { "ns", "netsync", "nets"},
        Description = "A command to interact with the NetworkSync System",
        Usage = "GET = <?key>, SET = <key> <class> <json>",
        Permission = "synapse.command.networksync",
        Platforms = new[] { Platform.RemoteAdmin,Platform.ServerConsole }
    )]
    public class SynapseNetworkSyncCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if (context.Arguments.Count == 0)
            {
                var awaiter =    Server.Get.NetworkManager.Client.RequestAllNetworkVars().GetAwaiter();
                awaiter.OnCompleted(() =>
                {
                    var entry = awaiter.GetResult();
                    Server.Get.Logger.Info($"\n{entry.Humanize()}");
                });
                result.Message = "Sending Get-Request...";
                result.State = CommandResultState.Ok;
            } else if (context.Arguments.Count == 1)
            {
                var key = context.Arguments.At(0);
                var awaiter = Server.Get.NetworkManager.Client.RequestNetworkVar<NetworkSyncEntry>(key).GetAwaiter();
                awaiter.OnCompleted(() =>
                {
                    var entry = awaiter.GetResult();
                    Server.Get.Logger.Info($"\n{entry.Humanize()}");
                });
                result.Message = "Sending Get-Request...";
                result.State = CommandResultState.Ok;
            } else if (context.Arguments.Count >= 3)
            {
                var data = string.Join(" ", context.Arguments.Where((x, i) => i >= 2));
                var entry = new NetworkSyncEntry
                {
                    Key = context.Arguments.At(0),
                    Class = context.Arguments.At(1),
                    Data = data
                };
                Server.Get.Logger.Info(entry.Humanize());

                var awaiter = Server.Get.NetworkManager.Client.Post<StatusedResponse,NetworkSyncEntry>($"/networksync?key={entry.Key}", entry).GetAwaiter(); 
                awaiter.OnCompleted(() =>
                {
                    var r = awaiter.GetResult();
                    Server.Get.Logger.Info($"\n{r.Humanize()}");
                });
                result.Message = $"Sending Set-Request with data {data}";
                result.State = CommandResultState.Ok;
            }
            return result;
        }
    }
}
