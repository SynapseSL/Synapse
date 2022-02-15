namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "Shematic",
        Aliases = new string[] { },
        Description = "Spawns a shematic",
        Permission = "synapse.command.shematic",
        Platforms = new[] { Platform.RemoteAdmin },
        Usage = "shematic id",
        Arguments = new[] { "Shematic ID" }
        )]
    public class SynapseSpawnShematic : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            if (context.Arguments.Count == 0) return new CommandResult
            {
                Message = "Missing parameter! Usage: shematic id",
                State = CommandResultState.Error
            };

            if (!int.TryParse(context.Arguments.At(0), out var id)) return new CommandResult
            {
                Message = "Invalid ID",
                State = CommandResultState.Error
            };

            Server.Get.Shematic.SpawnShematic(id, context.Player.Position);

            return new CommandResult
            {
                Message = "Shematic spawned",
                State = CommandResultState.Ok
            };
        }
    }
}
