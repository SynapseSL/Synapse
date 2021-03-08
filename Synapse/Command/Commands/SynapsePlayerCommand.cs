using Swan;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "SynapsePlayer",
        Aliases = new[] {"sp", "splayer"},
        Description = "Get infos on local and network players",
        Usage = "sp <?player>",
        Permission = "synapse.command.synapseplayer",
        Platforms = new[] {Platform.RemoteAdmin, Platform.ServerConsole}
    )]
    public class SynapsePlayerCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if (context.Arguments.IsEmpty())
            {
                var awaiter = Server.Get.NetworkManager.Client.GetAllPlayers().GetAwaiter();
                awaiter.OnCompleted(() =>
                {
                    var result = awaiter.GetResult();
                    context.Player.SendRAConsoleMessage($"Players:\n{result.Humanize()}");
                });
            }

            result.State = CommandResultState.Ok;
            result.Message = "Getting players...";
            return result;
        }
    }
}