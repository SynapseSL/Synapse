using System.Linq;
using HarmonyLib;
using Synapse.Network;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "NetworkBroadcast",
        Aliases = new[] {"nbc"},
        Description = "Sends a networked broadcast to a player",
        Usage = "nbc <userid> <message>",
        Permission = "synapse.command.networkbroadcast",
        Platforms = new[] {Platform.RemoteAdmin, Platform.ServerConsole}
    )]
    public class SynapseNetworkBroadcastCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if (context.Arguments.Count >= 2)
            {
                var id = context.Arguments.At(0);
                var msg = context.Arguments.Skip(1).Join(delimiter: " ");
                var awaiter = NetworkPlayer.FromUID(id).SendBroadcastMessage(msg).GetAwaiter();
                awaiter.OnCompleted(() => { context.Player.SendRAConsoleMessage("Message delivered"); });
            }
            else
            {
                result.Message = "Too few arguments";
                result.State = CommandResultState.Error;
                return result;
            }

            result.Message = "Sending Broadcast...";
            return result;
        }
    }
}