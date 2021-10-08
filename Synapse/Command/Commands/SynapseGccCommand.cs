using System;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "CollectGarbage",
        Aliases = new[] {"gc", "gcc"},
        Description = "Runs the garbage collector",
        Usage = "gcc",
        Permission = "synapse.command.gcc",
        Platforms = new[] {Platform.RemoteAdmin, Platform.ServerConsole}
    )]
    public class SynapseGccCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            var before = GC.GetTotalMemory(true);
            GC.Collect();
            var after = GC.GetTotalMemory(true);

            result.State = CommandResultState.Ok;
            result.Message = $"Garbage Collector freed {after - before}Bytes";
            return result;
        }
    }
}