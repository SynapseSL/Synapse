using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using PlayerRoles;
using Synapse3.SynapseModule.Config;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseCommand(
    CommandName = "ScpProximity",
    Aliases = new []{ "sp" },
    Platforms = new [] { CommandPlatform.PlayerConsole }
    )]
public class ScpProximityCommand : SynapseCommand
{
    private readonly SynapseConfigService _config;
    public ScpProximityCommand(SynapseConfigService config) => _config = config;
    
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        if (context.Player.Team != Team.SCPs)
        {
            result.Response = _config.Translation.Get(context.Player).NotScpProximity;
            result.StatusCode = CommandStatusCode.Forbidden;
            return;
        }

        if (!_config.GamePlayConfiguration.SpeakingScp.Contains(context.Player.RoleID) &&
            !context.Player.HasPermission("synapse.scp-proximity"))
        {
            result.Response = _config.Translation.Get(context.Player).NotAllowedProximity;
            result.StatusCode = CommandStatusCode.Forbidden;
            return;
        }

        context.Player.MainScpController.ProximityChat = !context.Player.MainScpController.ProximityChat;
        var translation = _config.Translation.Get(context.Player);

        result.Response = context.Player.MainScpController.ProximityChat
            ? translation.EnableProximity
            : translation.DisableProximity;
    }
}