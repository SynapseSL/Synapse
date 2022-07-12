using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseRaCommand(
    CommandName = "SetClass",
    Aliases = new[] { "sc", "class" },
    Parameters = new[] { "Player", "RoleID" },
    Description = "A command to set the class of a Player",
    Permission = "synapse.command.setclass",
    Platforms = new [] { CommandPlatform.RemoteAdmin, CommandPlatform.ServerConsole }
)]
public class SetClassCommand : SynapseCommand
{
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        if(context.Arguments.Length < 2)
        {
            result.Response = "Missing Parameters! Command Usage: setclass player RoleID";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        var player = Synapse.Get<PlayerService>().GetPlayer(context.Arguments[0]);
        if(player == null)
        {
            result.Response = "No Player was found!";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        if(!int.TryParse(context.Arguments[1],out var id))
        {
            result.Response = "Invalid parameter for RoleID";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        
        if (!Synapse.Get<RoleService>().IsIdRegistered(id))
        {
            result.Response = "No Role with this RoleID was found";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        player.RemoveCustomRole(DespawnReason.ForceClass);

        if (id is >= 0 and <= 17)
            player.RoleType = (RoleType)id;
        else
            player.RoleID = id;
        

        result.Response = "Player Role was set";
        result.StatusCode = CommandStatusCode.Ok;
    }
}