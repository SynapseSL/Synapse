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
    private readonly PlayerEvents _player;
    private readonly KeyBindService _keyBind;
    private readonly SynapseConfigService _config;

    private Dictionary<SynapsePlayer, string> _bindingPlayer = new();


    public KeyBindCommand(SynapseConfigService config, KeyBindService keyBind, PlayerEvents player)
    {
        _config = config;
        _keyBind = keyBind;
        _player = player;
    }


    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        var player = context.Player;
        if (_bindingPlayer.ContainsKey(player))
        {
            var bind = _keyBind.GetBind(_bindingPlayer[player]);
            var key = bind.Attribute.Bind;
            if (!player._commandKey.TryGetValue(key, out var commands))
                player._commandKey[key] = commands = new List<IKeyBind>();
            commands.Add(bind);
            _bindingPlayer.Remove(player);
            result.Response = player.GetTranslation(_config.Translation).KeyBindCommandUndo;
            return;
        }

        if (context.Arguments.Length == 0)
        {
            var message = player.GetTranslation(_config.Translation).KeyBindCommandGetCommand;
            var commands = "";

            foreach (var binds in player.CommandKey)
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
        var found = false;
        foreach (var binds in player._commandKey)
        {
            var commands = binds.Value;
            var lengthCommands = commands.Count;
            for (int i = 0; i < lengthCommands; i++)
            {
                var command = commands[i];
                if (command.Attribute.CommandName.ToLower() == commandName)
                {
                    found = true;
                    break;
                }
            }

            if (found) break;
        }

        if (!found)
        {
            result.Response = player.GetTranslation(_config.Translation).KeyBindCommandInvalidKey;
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        result.Response = player.GetTranslation(_config.Translation).KeyBindCommandSelectKey;
        _bindingPlayer.Add(player, commandName);
        _player.KeyPress.Subscribe(OnSelectKey);
    }

    private void OnSelectKey(KeyPressEvent keyPress)
    {
        var player = keyPress.Player;

        if (!_bindingPlayer.ContainsKey(player)) return;

        var commandName = _bindingPlayer[player];
        var key = keyPress.KeyCode;
        if (!player._commandKey.TryGetValue(key, out var commands))
            player._commandKey[key] = commands = new List<IKeyBind>();

        commands.Add(_keyBind.GetBind(commandName));
        player.SendConsoleMessage($"Command: \"{commandName}\" bind to \"{key}\"");

        _player.KeyPress.Unsubscribe(OnSelectKey);
    }

}
