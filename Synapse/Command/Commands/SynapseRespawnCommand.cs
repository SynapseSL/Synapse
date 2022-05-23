using System.Linq;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "Respawn",
        Aliases = new string[] { "spawn" },
        Description = "spawns a specific team,",
        Usage = "respawn teamid size",
        Permission = "synapse.command.respawn",
        Platforms = new[] { Platform.ClientConsole, Platform.RemoteAdmin, Platform.ServerConsole },
        Arguments = new[] { "TeamID" }
    )]
    public class SynapseRespawnCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            if (context.Arguments.Count < 1) return new()
            {
                Message = "Missing parameter! Usage: spawn teamid",
                State = CommandResultState.Error
            };

            if (!int.TryParse(context.Arguments.At(0), out var id)) return new()
            {
                Message = "Invalid Team ID",
                State = CommandResultState.Error
            };
            var players = RoleType.Spectator.GetPlayers().Where(x => !x.OverWatch).ToList();

            if (context.Arguments.Count > 1 && int.TryParse(context.Arguments.At(1), out var size))
            {
                if (players.Count > size)
                    players = players.GetRange(0, size);
            }

            if (players.Count < 1) return new()
            {
                Message = "Not enough players to respawn",
                State = CommandResultState.Error
            };

            Server.Get.TeamManager.SpawnTeam(id, players);
            return new()
            {
                Message = "Team was spawned",
                State = CommandResultState.Ok
            };
        }
    }
}