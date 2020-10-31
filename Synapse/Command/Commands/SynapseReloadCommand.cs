namespace Synapse.Command.Commands
{

    [CommandInformation(
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
                result.Message = "You don't have permission to use this command"; 
                return result;
            }

            try
            {
                Server.Get.Reload();
                result.State = CommandResultState.Ok;
                result.Message = "Reloading was successful";
            }
            catch
            {
                result.State = CommandResultState.Error;
                result.Message = "Error occured while reloading";
            }

            return result;
        }
    }
}
