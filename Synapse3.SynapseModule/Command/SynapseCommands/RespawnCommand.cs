using System.Linq;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;
using Synapse3.SynapseModule.Teams;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseRaCommand(
    CommandName = "Respawn",
    Aliases = new[] { "spawn" },
    Parameters = new[] { "teamID", "size" },
    Description = "spawns a specific team",
    Permission = "synapse.command.respawn",
    Platforms = new [] { CommandPlatform.RemoteAdmin, CommandPlatform.ServerConsole }
)]
public class RespawnCommand : SynapseCommand
{
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        if (context.Arguments.Length < 1)
        {
            result.Response = "Missing parameter! Usage: spawn teamID";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        if (!int.TryParse(context.Arguments[0], out var id))
        {
            result.Response = "Invalid Team ID";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }
        
        var players = Synapse.Get<PlayerService>().GetPlayers(player => player.RoleType == RoleType.Spectator && !player.OverWatch).ToList();
        if (context.Arguments.Length > 1 && int.TryParse(context.Arguments[1], out var size))
        {
            if (players.Count > size)
                players = players.GetRange(0, size);
        }

        if (players.Count < 1)
        {
            result.Response = "Not enough players to respawn";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        Synapse.Get<TeamService>().SpawnTeam(id, players);
        result.Response = "Team was spawned";
        result.StatusCode = CommandStatusCode.Error;
    }
}
