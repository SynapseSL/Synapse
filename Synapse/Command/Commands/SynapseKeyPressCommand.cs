using System;
using System.Linq;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "Keypress",
        Aliases = new[] { "key" },
        Description = "A command for the KeyPressEvent from Synapse",
        Usage = "Read the wiki",
        Permission = "",
        Platforms = new[] { Platform.ClientConsole }
    )]
    public class SynapseKeyPressCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if (context.Player == Server.Get.Host)
            {
                result.Message = "The ServerConsole cant use this command";
                result.State = CommandResultState.Error;
                return result;
            }

            if (context.Arguments.Count < 1)
            {
                result.Message = "Use .keypress sync in order to sync your binds and use all Features of the Plugins!";
                result.State = CommandResultState.Error;
                return result;
            }

            switch (context.Arguments.FirstOrDefault().ToUpper())
            {
                case "SYNC":
                    foreach (var key in (KeyCode[])Enum.GetValues(typeof(KeyCode)))
                        context.Player.ClassManager.TargetChangeCmdBinding(context.Player.Connection, key, $".keypress send {(int)key}");
                    result.State = CommandResultState.Ok;
                    result.Message = "All Keys was synced";
                    return result;

                case "SEND":
                    if (!Enum.TryParse<KeyCode>(context.Arguments.ElementAt(1), out var key2))
                    {
                        result.Message = "Invalid KeyBind! If they are binded by Synapse please report this!";
                        result.State = CommandResultState.Error;
                        return result;
                    }

                    try
                    {
                        Server.Get.Events.Player.InvokePlayerKeyPressEvent(context.Player, key2);
                    }
                    catch (Exception e)
                    {
                        Logger.Get.Error($"Synapse-Event: PlayerKeyPress failed!!\n{e}");
                    }

                    result.Message = "Key was accepted";
                    result.State = CommandResultState.Ok;
                    return result;

                default:
                    result.Message = "Use .key sync in order to sync your binds and use all Features of the Plugins!";
                    result.State = CommandResultState.Error;
                    return result;
            }
        }
    }
}
