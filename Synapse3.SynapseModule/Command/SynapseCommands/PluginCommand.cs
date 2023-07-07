using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using PluginAPI.Core;
using Synapse3.SynapseModule.Role;

namespace Synapse3.SynapseModule.Command.SynapseCommands;


[SynapseRaCommand(
    CommandName = "Plugins",
    Aliases = new[] { "pls, plugin" },
    Parameters = new[] { "(pluginName)" },
    Description = "A command which provides information about the currently installed Plugins",
    Permission = "synapse.command.plugins",
    Platforms = new[] { CommandPlatform.PlayerConsole, CommandPlatform.RemoteAdmin, CommandPlatform.ServerConsole }
)]
public class PluginCommand : SynapseCommand
{
    private readonly ServerService _server;

    public PluginCommand(ServerService server)
    {
        _server = server;
    }

    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        if (context.Arguments.Length > 0)
        {
            var pluginName = string.Join(" ", context.Arguments);

            var plugin = _server.Plugins.FirstOrDefault(x =>
                string.Equals(x.Attribute.Name, pluginName, StringComparison.OrdinalIgnoreCase));

            if (plugin == null)
            {
                result.Response = "No Plugin was found";
                result.StatusCode = CommandStatusCode.Error;
                return;
            }

            result.Response = $"\n{plugin.Attribute.Name}" +
                              $"\n    - Description: {SplitDescription(plugin.Attribute.Description, context.Platform)}" +
                              $"\n    - Author: {plugin.Attribute.Author}" +
                              $"\n    - Version: {plugin.Attribute.Version}" +
                              $"\n    - Repository: {plugin.Attribute.Repository}" +
                              $"\n    - Website: {plugin.Attribute.Website}";

            result.StatusCode = CommandStatusCode.Ok;
            return;
        }

        result.Response = "All Plugins:";

        foreach (var plugi in _server.Plugins)
            result.Response += $"\n{plugi.Attribute.Name} Version: {plugi.Attribute.Version} by {plugi.Attribute.Author}";

        result.StatusCode = CommandStatusCode.Ok;
    }

    private string SplitDescription(string message, CommandPlatform platform)
    {
        var count = 0;
        var msg = "";

        foreach (var word in message.Split(' '))
        {
            count += word.Length;

            if (count > _maxLetters[platform])
            {
                msg += "\n                ";
                count = 0;
            }

            if (msg == string.Empty)
            {
                msg += word;
            }
            else
            {
                msg += " " + word;
            }
        }

        return msg;
    }

    private readonly Dictionary<CommandPlatform, int> _maxLetters = new()
    {
        { CommandPlatform.PlayerConsole, 50 },
        { CommandPlatform.RemoteAdmin, 50 },
        { CommandPlatform.ServerConsole, 75 }
    };
}