using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MEC;
using Neuron.Modules.Commands;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Elevators;
using Synapse3.SynapseModule.Map.Objects;
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
        result.Response = "Test";


        /*
        var config = new SchematicConfiguration()
        {
            Name = "ExampleRoom",
            ID = 1,
            Primitives = new List<SchematicConfiguration.PrimitiveConfiguration>()
            {
                new SchematicConfiguration.PrimitiveConfiguration()
                {
                    PrimitiveType = PrimitiveType.Cube,
                    Position = Vector3.down,
                    Scale = new SerializedVector3(10f, 0.2f, 10f),
                    Color = new SerializedColor(0.5f, 0.5f, 0.5f, 1f)
                },
                new SchematicConfiguration.PrimitiveConfiguration()
                {
                    PrimitiveType = PrimitiveType.Cube,
                    Position = Vector3.up * 4,
                    Scale = new SerializedVector3(10f, 0.2f, 10f),
                    Color = new SerializedColor(0.5f, 0.5f, 0.5f, 1f)
                },
                new SchematicConfiguration.PrimitiveConfiguration()
                {
                    PrimitiveType = PrimitiveType.Cube,
                    Position = new SerializedVector3(5f, 1.5f, 0f),
                    Scale = new SerializedVector3(0.2f, 5f, 10f),
                    Color = new SerializedColor(0.5f, 0.5f, 0.5f, 1f)
                },
                new SchematicConfiguration.PrimitiveConfiguration()
                {
                    PrimitiveType = PrimitiveType.Cube,
                    Position = new SerializedVector3(-5f, 1.5f, 0f),
                    Scale = new SerializedVector3(0.2f, 5f, 10f),
                    Color = new SerializedColor(0.5f, 0.5f, 0.5f, 1f)
                },
                new SchematicConfiguration.PrimitiveConfiguration()
                {
                    PrimitiveType = PrimitiveType.Cube,
                    Position = new SerializedVector3(0f, 1.5f, 5f),
                    Scale = new SerializedVector3(10f, 5f, 0.2f),
                    Color = new SerializedColor(0.5f, 0.5f, 0.5f, 0.3f)
                },
                new SchematicConfiguration.PrimitiveConfiguration()
                {
                    PrimitiveType = PrimitiveType.Cube,
                    Position = new SerializedVector3(3f, 1.5f, -5f),
                    Scale = new SerializedVector3(4f, 5f, 0.2f),
                    Color = new SerializedColor(0.5f, 0.5f, 0.5f, 1f)
                },
                new SchematicConfiguration.PrimitiveConfiguration()
                {
                    PrimitiveType = PrimitiveType.Cube,
                    Position = new SerializedVector3(-3f, 1.5f, -5f),
                    Scale = new SerializedVector3(4f, 5f, 0.2f),
                    Color = new SerializedColor(0.5f, 0.5f, 0.5f, 1f)
                },
                new SchematicConfiguration.PrimitiveConfiguration()
                {
                    PrimitiveType = PrimitiveType.Cube,
                    Position = new SerializedVector3(0f, 3.2f, -5f),
                    Scale = new SerializedVector3(2f, 1.6f, 0.2f),
                    Color = new SerializedColor(0.5f, 0.5f, 0.5f, 1f)
                }
            },
            Lights = new List<SchematicConfiguration.LightSourceConfiguration>
            {
                new SchematicConfiguration.LightSourceConfiguration()
                {
                    Color = new SerializedColor(1f, 1f, 1f, 1f),
                    LightIntensity = 0.5f,
                    LightRange = 50,
                    LightShadows = false
                }
            },
            Doors = new List<SchematicConfiguration.DoorConfiguration>
            {
                new SchematicConfiguration.DoorConfiguration()
                {
                    Position = new SerializedVector3(0f, -1f, -5f),
                    DoorType = SynapseDoor.SpawnableDoorType.Lcz,
                }
            }
        };
        Synapse.Get<SchematicService>().SaveConfiguration(config);
        Synapse.Get<RoomService>().RegisterCustomRoom<RoomService.TestRoom>();
        var room = Synapse.Get<RoomService>().SpawnCustomRoom(100,context.Player.Position);

        room.Position = new Vector3(0f, 1020f, 0f);
        */
    }
}