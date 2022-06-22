using Neuron.Modules.Commands;
using Synapse3.SynapseModule.CustomRole;
using Synapse3.SynapseModule.Map.Objects;
using UnityEngine;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseRaCommand(
    CommandName = "Test",
    Aliases = new []{ "te" },
    Description = "Command for testing purposes",
    Permission = "synapse.test",
    Platforms = new[] { CommandPlatform.PlayerConsole, CommandPlatform.RemoteAdmin , CommandPlatform.ServerConsole },
    Parameters = new []{ "Test" }
    )]
public class TestCommand : SynapseCommand
{
    private CustomRoleService.TestClass role = new ();
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        result.Response = "Test message";

        new SynapseLocker(SynapseLocker.LockerType.ScpPedestal, context.Player.Position, context.Player.Rotation,
            Vector3.one);
    }
}