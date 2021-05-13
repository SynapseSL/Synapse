using System.Linq;
using HarmonyLib;
using Synapse.Network;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "NetworkKick",
        Aliases = new[] {"nkick"},
        Description = "Kicks a networked player",
        Usage = "nkick <userid> <?message>",
        Permission = "synapse.command.networkkick",
        Platforms = new[] {Platform.RemoteAdmin, Platform.ServerConsole}
    )]
    public class SynapseNetworkKickCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if (context.Arguments.Count == 1)
            {
                var id = context.Arguments.At(0);
                var awaiter = NetworkPlayer.FromUID(id).Kick("").GetAwaiter();
                awaiter.OnCompleted(() => { context.Player.SendRAConsoleMessage("Player kicked"); });
            }

            if (context.Arguments.Count >= 2)
            {
                var id = context.Arguments.At(0);
                var msg = context.Arguments.Skip(1).Join(delimiter: " ");
                var awaiter = NetworkPlayer.FromUID(id).Kick(msg).GetAwaiter();
                awaiter.OnCompleted(() => { context.Player.SendRAConsoleMessage("Player kicked"); });
            }
            else
            {
                result.Message = "Too few arguments";
                result.State = CommandResultState.Error;
                return result;
            }

            result.Message = "Kicking Player...";
            return result;
        }
    }
}