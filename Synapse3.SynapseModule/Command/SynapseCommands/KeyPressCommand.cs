using System;
using System.Linq;
using Neuron.Core.Logging;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseCommand(
    CommandName = "KeyPress",
    Aliases = new[] { "key" },
    Description = "A command for the KeyPressEvent from Synapse",
    Permission = "",
    Platforms = new [] { CommandPlatform.PlayerConsole }
)]
public class KeyPressCommand : SynapseCommand
{
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        if(context.IsAdmin)
        {
            result.Response = "The ServerConsole cant use this command";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        if(context.Arguments.Length < 1)
        {
            result.Response = "Use .keypress sync in order to sync your binds and use all Features of the Plugins!";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        switch (context.Arguments.FirstOrDefault()?.ToUpper())
        {
            case "SYNC":
                foreach (var key in (KeyCode[])Enum.GetValues(typeof(KeyCode)))
                    context.Player.ClassManager.TargetChangeCmdBinding(context.Player.Connection, key,
                        $".keypress send {(int)key}");
                result.Response = "All Keys was synced";
                result.StatusCode = CommandStatusCode.Ok;
                return;

            case "SEND":
                if (!Enum.TryParse<KeyCode>(context.Arguments.ElementAt(1), out var key2))
                {
                    result.Response = "Invalid KeyBind! If they are bound by Synapse please report this!";
                    result.StatusCode = CommandStatusCode.Error;
                    return;
                }

                try
                {
                    var ev = new KeyPressEvent(context.Player, key2);
                    Synapse.Get<PlayerEvents>().KeyPress.Raise(ev);
                }
                catch (Exception ex)
                {
                    NeuronLogger.For<Synapse>().Error($"Sy3 Command: PlayerKeyPress failed\n{ex}");
                }
                result.Response = "Key was accepted";
                result.StatusCode = CommandStatusCode.Ok;
                return;

            default:
                result.Response = "Use .key sync in order to sync your binds and use all Features of the Plugins!";
                result.StatusCode = CommandStatusCode.Error;
                return;
        }
    }
}