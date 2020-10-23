using System.Collections.Generic;
using System.Linq;

namespace Synapse.Command.Commands
{
    
    [CommandInformations(
        Name = "help",
        Aliases = new[]{"h"},
        Description = "Shows all available commands with usage description",
        Usage = "help {Optional Command Name for a specific Command}",
        Permission = "synapse.command.help",
        Platforms = new[] {Platform.ClientConsole, Platform.RemoteAdmin, Platform.ServerConsole}
    )]
    public class SynapseHelpCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if (!context.Player.HasPermission("synapse.command.help"))
            {
                result.State = CommandResultState.NoPermission;
                result.Message = "You have no Permission to execute this Command!";
                return result;
            }



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

            commandlist = commandlist.Where(x => context.Player.HasPermission(x.Permission) || string.IsNullOrWhiteSpace(x.Permission) || x.Permission.ToUpper() == "NONE").ToList();

            if(context.Arguments.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(context.Arguments.First()))
                    goto A_001;

                var command = commandlist.FirstOrDefault(x => x.Name.ToLower() == context.Arguments.First());

                if(command == null)
                {
                    result.State = CommandResultState.Error;
                    result.Message = "No Command with this Name found";
                    return result;
                }

                string platforms = "{ ";
                string aliases = "{ ";
                foreach (var x in command.Platforms)
                    platforms += $"{x},";

                foreach (var y in command.Aliases)
                    aliases += $"{y},";

                platforms += " }";
                aliases += " }";

                result.Message = $"\n{command.Name}\n    - Permission: {command.Permission}\n    - Description: {command.Description}\n    - Usage: {command.Usage}\n    - Platforms: {platforms}\n    - Aliases: {aliases}";

                result.State = CommandResultState.Ok;
                return result;
            }

            A_001:
            var msg = $"All Commands which you can execute for {context.Platform}:";

            foreach (var command in commandlist)
            {
                string alias = "{ ";
                foreach (var ali in command.Aliases)
                    alias += $"{ali},";
                alias += " }";


                msg += $"\n{command.Name}:\n    -Usage: {command.Usage}\n    -Description: {command.Description}\n    -Aliases: {alias}";
            }

            result.Message = msg;
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
