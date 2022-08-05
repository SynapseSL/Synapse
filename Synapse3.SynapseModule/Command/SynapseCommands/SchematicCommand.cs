using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using Synapse3.SynapseModule.Map.Schematic;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseRaCommand(
    CommandName = "Schematic",
    Aliases = new string[] { },
    Parameters = new[] { "ID" },
    Description = "Spawns a schematic",
    Permission = "synapse.command.schematic",
    Platforms = new [] { CommandPlatform.RemoteAdmin }
)]
public class SchematicCommand : SynapseCommand
{
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        if (context.Arguments.Length == 0)
        {
            result.Response = "Missing parameter! Usage: schematic id";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        if (!uint.TryParse(context.Arguments[0], out var id))
        {
            result.Response = "Invalid ID";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        
        if (!Synapse.Get<SchematicService>().IsIDRegistered(id))
        {
            result.Response = "No Schematic with this ID was found";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }
        
        Synapse.Get<SchematicService>().SpawnSchematic(id, context.Player.Position);

        result.Response = "Schematic spawned";
        result.StatusCode = CommandStatusCode.Ok;
    }
}