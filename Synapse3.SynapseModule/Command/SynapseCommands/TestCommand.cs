using System.Collections.Generic;
using Neuron.Modules.Commands;
using Synapse3.SynapseModule.Enums;
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
        result.Response = "Spawn Schematic!";

        if (context.Arguments.Length > 0)
        {
            new SynapseGenerator(context.Player.Position, Quaternion.identity, Vector3.one);
            return;
        }
        
        var config = new SchematicConfiguration()
        {
            Name = "TestCommand",
            ID = 0,
            Primitives = new List<SchematicConfiguration.PrimitiveConfiguration>
            {
                new SchematicConfiguration.PrimitiveConfiguration()
                {
                    Color = Color.red,
                    PrimitiveType = PrimitiveType.Plane
                }
            },
            Doors = new List<SchematicConfiguration.DoorConfiguration>
            {
                new SchematicConfiguration.DoorConfiguration()
                {
                    DoorType = SynapseDoor.SpawnableDoorType.EZ,
                    Position = Vector3.up * 5,
                }
            },
            Generators = new List<SchematicConfiguration.SimpleUpdateConfig>
            {
                new SchematicConfiguration.SimpleUpdateConfig()
                {
                    Position = Vector3.right * 5
                }
            },
            Lights = new List<SchematicConfiguration.LightSourceConfiguration>
            {
                new SchematicConfiguration.LightSourceConfiguration()
                {
                    Color = Color.blue,
                    LightIntensity = 20,
                    LightRange = 10,
                    Position = Vector3.up
                }
            },
            Lockers = new List<SchematicConfiguration.LockerConfiguration>
            {
                new SchematicConfiguration.LockerConfiguration()
                {
                    Position = Vector3.right * -5,
                    LockerType = SynapseLocker.LockerType.StandardLocker,
                    DeleteDefaultItems = true
                }
            },
            Ragdolls = new List<SchematicConfiguration.RagdollConfiguration>
            {
                new SchematicConfiguration.RagdollConfiguration
                {
                    Nick = "Test",
                    Position = Vector3.forward * 5,
                    RoleType = RoleType.Scientist,
                    DamageType = DamageType.Asphyxiated
                }
            },
            Targets = new List<SchematicConfiguration.TargetConfiguration>
            {
                new SchematicConfiguration.TargetConfiguration()
                {
                    Position = Vector3.forward * -5,
                    TargetType = SynapseTarget.TargetType.Binary,
                }
            },
            WorkStations = new List<SchematicConfiguration.SimpleUpdateConfig>
            {
                new SchematicConfiguration.SimpleUpdateConfig()
                {
                    Position = Vector3.up * -5,
                }
            }
        };
        var schematic = new SynapseSchematic(config);
        schematic.Position = context.Player.Position;
    }
}