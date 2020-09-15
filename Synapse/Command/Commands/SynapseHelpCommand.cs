using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Command.Commands
{
    
    [CommandInformations(
        Name = "help",
        Aliases = new[]{"h"},
        Description = "Shows all available commands with usage description",
        Usage = "help",
        Permission = "synapse.commands.help",
        Platforms = new[] {Platform.ClientConsole, Platform.RemoteAdmin, Platform.ServerConsole}
    )]
    public class SynapseHelpCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {

            var result = new CommandResult
            {
                State = CommandResultState.Ok,
                Message = "Help Message!"
            };

            return result;
        }
    }
}
