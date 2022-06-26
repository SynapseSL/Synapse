using System.Linq;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseRaCommand(
    CommandName = "Plugin",
    Aliases = new[] { "pl, plugins" },
    Parameters = new string[] { "(pluginName)" },
    Description = "A command which provides information about the currently installed Plugins",
    Permission = "synapse.command.plugins",
    Platforms = new [] { CommandPlatform.PlayerConsole, CommandPlatform.RemoteAdmin, CommandPlatform.ServerConsole }
)]
public class PluginCommand : SynapseCommand
{
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        if(context.Arguments.Length > 0)
        {
            // TODO: Wait for Helights response
            // var pl = SynapseController.PluginLoader.Plugins.FirstOrDefault(x => x.Name.ToUpper() == context.Arguments.First().ToUpper());
            // if(pl == null)
            // {
            //     result.Response = "No Plugin was found";
            //     result.StatusCode = CommandStatusCode.Error;
            //     return;
            // }
            //
            // result.Response = $"\n{pl.Name}" +
            //                   $"\n    - Description: {pl.Description}" +
            //                   $"\n    - Author: {pl.Author}" +
            //                   $"\n    - Version: {pl.Version}" +
            //                   $"\n    - Based on Synapse Version: {pl.SynapseMajor}.{pl.SynapseMinor}.{pl.SynapsePatch}";
            //
            // if (context.Player.HasPermission("synapse.debug"))
            // {
            //     result.Response += $"\n    - LoadPriority: {pl.LoadPriority}" +
            //                        $"\n    - Is Shared: {pl.shared}";
            // }
            //
            // result.StatusCode = CommandStatusCode.Ok;
            // return;
        }

        // result.Response = "All Plugins:";
        // foreach(var pl in SynapseController.PluginLoader.Plugins)
        //     result.Response += $"\n{pl.Name} Version: {pl.Version} by {pl.Author}";
        //
        // result.StatusCode = CommandStatusCode.Ok;
    }
}