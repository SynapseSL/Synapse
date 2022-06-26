using System.Collections.Generic;
using Neuron.Modules.Commands;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Map.Schematic;
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
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        result.Response = "Save";

        var config = new SchematicConfiguration()
        {
            ID = 100,
            Name = "Test",
            Primitives = new List<SchematicConfiguration.PrimitiveConfiguration>()
            {
                new SchematicConfiguration.PrimitiveConfiguration()
                {
                    Color = Color.black,
                    Position = Vector3.up * 10
                }
            }
        };

        Synapse.Get<SchematicService>().SaveConfiguration(config);
    }
}