using MEC;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using PluginAPI.Core;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.KeyBind;
using Synapse3.SynapseModule.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;
using static CmdBinding;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseCommand(
    CommandName = "KeyBind",
    Aliases = new[] { "bind" },
    Description = "Gives/Sets your keyBind that should be used for you on this Server",
    Platforms = new[] { CommandPlatform.PlayerConsole }
)]
internal class KeyBindCommand : SynapseCommand
{
    private readonly KeyBindService _keyBind;
    private readonly SynapseConfigService _config;

    public KeyBindCommand(SynapseConfigService config, KeyBindService keyBind)
    {
        _config = config;
        _keyBind = keyBind;
    }


    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        if (context.Arguments.Length == 0)
        {
            var message = context.Player.GetTranslation(_config.Translation).KeyBindCommandGetCommand;
            var commands = "";

            foreach (var binds in context.Player.Binds)
            {
                foreach (var command in binds.Value)
                {
                    commands += $"\n{command.Attribute.CommandName}";
                    commands += $"\n    Bind: {binds.Key}";
                    commands += $"\n    Description: {command.Attribute.CommandDescription}";
                }
            }
            result.Response = string.Format(message, commands);
            return;
        }

        var commandName = string.Join(" ", context.Arguments.ToArray()).ToLower();
        IKeyBind bind = null;
        foreach (var defaultBind in _keyBind.DefaultBinds)
        {
            if (!string.Equals(defaultBind.Attribute.CommandName, commandName,
                    StringComparison.OrdinalIgnoreCase)) continue;
            bind = defaultBind;
            break;
        }

        if (bind == null)
        {
            result.Response = _config.Translation.Get(context.Player).KeyBindCommandInvalidKey;
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        result.Response = _config.Translation.Get(context.Player).KeyBindCommandSelectKey;
        _keyBind.NewBinding[context.Player] = bind;
    }
}
