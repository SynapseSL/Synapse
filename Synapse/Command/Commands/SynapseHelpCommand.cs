using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Command.Commands
{

    [CommandInformation(
        Name = "Help",
        Aliases = new[] { "h" },
        Description = "Shows all available commands with usage description",
        Usage = "help {optional command name for a specific command}",
        Permission = "synapse.command.help",
        Platforms = new[] { Platform.ClientConsole, Platform.RemoteAdmin, Platform.ServerConsole }
    )]
    public class SynapseHelpCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            List<ICommand> commandlist;

            switch (context.Platform)
            {
                case Platform.ClientConsole:
                    commandlist = SynapseController.CommandHandlers.ClientCommandHandler.Commands;
                    break;

                case Platform.RemoteAdmin:
                    commandlist = SynapseController.CommandHandlers.RemoteAdminHandler.Commands;
                    break;

                case Platform.ServerConsole:
                    commandlist = SynapseController.CommandHandlers.ServerConsoleHandler.Commands;
                    break;

                default:
                    result.Message = "Information for this Console is not supported";
                    result.State = CommandResultState.Error;
                    return result;
            }

            commandlist = commandlist.Where(x => context.Player.HasPermission(x.Permission) || String.IsNullOrWhiteSpace(x.Permission) || x.Permission.ToUpper() == "NONE").ToList();

            if (context.Arguments.Count > 0 && !String.IsNullOrWhiteSpace(context.Arguments.First()))
            {
                var command = commandlist.FirstOrDefault(x => x.Name.Equals(context.Arguments.First(), StringComparison.OrdinalIgnoreCase));

                if (command is null)
                {
                    foreach (var c in commandlist.Where(c => c.Aliases.Any(alias => alias.Equals(context.Arguments.First(), StringComparison.OrdinalIgnoreCase))))
                    {
                        command = c;
                    }

                    if (command is null)
                    {
                        result.State = CommandResultState.Error;
                        result.Message = "No Command with this Name found";
                        return result;
                    }
                }

                var platforms = "{ " + String.Join(", ", command.Platforms) + " }";
                var aliases = "{ " + String.Join(", ", command.Aliases) + " }";

                result.Message = String.IsNullOrWhiteSpace(command.Permission)
                    ? $"\n{command.Name}\n    - Description: {command.Description}\n    - Usage: {command.Usage}\n    - Platforms: {platforms}\n    - Aliases: {aliases}"
                    : $"\n{command.Name}\n    - Permission: {command.Permission}\n    - Description: {command.Description}\n    - Usage: {command.Usage}\n    - Platforms: {platforms}\n    - Aliases: {aliases}";

                result.State = CommandResultState.Ok;
                return result;
            }

            var msg = $"All Commands which you can execute for {context.Platform}:";

            foreach (var command in commandlist)
            {
                var alias = "{ " + String.Join(", ", command.Aliases) + " }";

                msg += $"\n{command.Name}:\n    -Usage: {command.Usage}\n    -Description: {command.Description}\n    -Aliases: {alias}";
            }

            result.Message = msg;
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
