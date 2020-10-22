using System.Linq;

namespace Synapse.Command.Commands
{
    [CommandInformations(
        Name = "plugin",
        Aliases = new[] { "pl","plugins" },
        Description = "A Command which gives Informations about the currently installed Plugins",
        Usage = "\"plugin\" for a list of all plugins or \"plugin {pluginname}\" for informations of a special Plugin",
        Permission = "synapse.command.plugins",
        Platforms = new[] { Platform.ClientConsole,Platform.RemoteAdmin,Platform.ServerConsole }
    )]
    public class SynapsePluginCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if(context.Arguments.Count > 0)
            {
                var pl = SynapseController.PluginLoader.Plugins.FirstOrDefault(x => x.Name.ToUpper() == context.Arguments.First().ToUpper());
                if(pl == null)
                {
                    result.Message = "No Plugin was found";
                    result.State = CommandResultState.Error;
                    return result;
                }

                result.Message = $"\n{pl.Name}" +
                    $"\n    - Description: {pl.Description}" +
                    $"\n    - Author: {pl.Author}" +
                    $"\n    - Version: {pl.Version}" +
                    $"\n    - Based on Synapse Version: {pl.SynapseMajor}.{pl.SynapseMinor}.{pl.SynapsePatch}";

                if (context.Player.HasPermission("synapse.debug"))
                {
                    result.Message += $"\n    - LoadPriority: {pl.LoadPriority}" +
                    $"\n    - Is Shared: {pl.shared}";
                }

                result.State = CommandResultState.Ok;
                return result;
            }

            result.Message = "All Plugins:";
            foreach(var pl in SynapseController.PluginLoader.Plugins)
                result.Message += $"\n{pl.Name} Version: {pl.Version} by {pl.Author}";

            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
