namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "Schematic",
        Aliases = new string[] { },
        Description = "Spawns a schematic",
        Permission = "synapse.command.schematic",
        Platforms = new[] { Platform.RemoteAdmin },
        Usage = "schematic id",
        Arguments = new[] { "Schematic ID" }
        )]
    public class SynapseSchematicCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            if (context.Arguments.Count == 0)
            {
                return new CommandResult
                {
                    Message = "Missing parameter! Usage: schematic id",
                    State = CommandResultState.Error
                };
            }

            if (!System.Int32.TryParse(context.Arguments.At(0), out var id))
            {
                return new CommandResult
                {
                    Message = "Invalid ID",
                    State = CommandResultState.Error
                };
            }

            if (!Server.Get.Schematic.IsIDRegistered(id))
            {
                return new CommandResult
                {
                    Message = "No Schematic with this ID was found",
                    State = CommandResultState.Error
                };
            }

            _ = Server.Get.Schematic.SpawnSchematic(id, context.Player.Position);

            return new CommandResult
            {
                Message = "Schematic spawned",
                State = CommandResultState.Ok
            };
        }
    }
}
