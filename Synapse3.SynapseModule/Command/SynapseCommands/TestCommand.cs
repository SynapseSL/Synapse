using Neuron.Modules.Commands;
using Synapse3.SynapseModule.CustomRole;
using Synapse3.SynapseModule.Enums;

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

        switch (context.Arguments.Length)
        {
            case 0:
                context.Player.CustomRole = role;
                break;
                    
            case 1:
                context.Player.SpawnCustomRole(role,true);
                break;
            
            case 2:
                context.Player.SpawnCustomRole(role,false);
                break;
            
            case 3:
                context.Player.RemoveCustomRole(DespawnReason.Death);
                break;
            
            case 4:
                context.Player.RemoveCustomRole(DespawnReason.API);
                break;
            
            case 5:
                context.Player.RemoveCustomRole(DespawnReason.Forceclass);
                break;
            
            case 6:
                context.Player.CustomRole = null;
                break;
        }
    }
}