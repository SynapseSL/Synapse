namespace Synapse.Command.Commands
{

    [CommandInformations(
        Name = "Reload",
        Aliases = new[] { "rl" },
        Description = "Reloads all Plugins and Features of Synapse",
        Usage = "reload",
        Permission = "synapse.command.reload",
        Platforms = new[] { Platform.RemoteAdmin, Platform.ServerConsole }
    )]
    public class SynapseReloadCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if (!context.Player.HasPermission("synapse.command.reload"))
            {
                result.State = CommandResultState.NoPermission;
                result.Message = "You have no Permission to execute this Command!";
                return result;
            }

            try
            {
                Server.Get.Reload();
                result.State = CommandResultState.Ok;
                result.Message = "Reloading was succesfully";
            }
            catch
            {
                result.State = CommandResultState.Error;
                result.Message = "Error Occured while Reloading";
            }

            return result;
        }
    }
}
