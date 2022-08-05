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
        result.Response = "Test";

        var config = new SchematicConfiguration
        {
            Items = new List<SchematicConfiguration.ItemConfiguration>
            {
                new SchematicConfiguration.ItemConfiguration
                {
                    Scale = Vector3.one,
                    Position = Vector3.up * 1,
                    Rotation = Quaternion.identity,
                    ItemType = ItemType.Medkit,
                    CanBePickedUp = false,
                    Physics = false
                }
            },
            Primitives = new List<SchematicConfiguration.PrimitiveConfiguration>
            {
                new ()
                {
                    Color = Color.white,
                    Position = Vector3.up * 3,
                    PrimitiveType = PrimitiveType.Cube
                }
            },
            Doors = new List<SchematicConfiguration.DoorConfiguration>
            {
                new ()
                {
                    Locked = false,
                    Open = false,
                    Position = Vector3.right * 3,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    DoorType = SynapseDoor.SpawnableDoorType.Hcz
                }
            },
            Generators = new List<SchematicConfiguration.SimpleUpdateConfig>
            {
                new()
                {
                    Position = Vector3.left * 3,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one
                }
            },
            Dummies = new List<SchematicConfiguration.DummyConfiguration>
            {
                new SchematicConfiguration.DummyConfiguration
                {
                    Position = Vector3.forward * 3,
                    Name = "test",
                    Rotation = Quaternion.identity,
                    Role = RoleType.ClassD,
                    Scale = Vector3.one,
                    HeldItem = ItemType.None
                }
            },
            Lights = new List<SchematicConfiguration.LightSourceConfiguration>
            {
                new SchematicConfiguration.LightSourceConfiguration
                {
                    Position = Vector3.zero,
                    Color = Color.blue,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.zero,
                    LightIntensity = 1,
                    LightRange = 10,
                    LightShadows = true
                }
            },
            Lockers = new List<SchematicConfiguration.LockerConfiguration>
            {
                new ()
                {
                    Position = Vector3.back * 3,
                    Chambers = new List<SchematicConfiguration.LockerConfiguration.LockerChamber>(),
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    LockerType = SynapseLocker.LockerType.ScpPedestal
                }
            },
            Ragdolls = new List<SchematicConfiguration.RagdollConfiguration>
            {
                new SchematicConfiguration.RagdollConfiguration
                {
                    Nick = "test",
                    Position = Vector3.zero,
                    Rotation = Quaternion.identity,
                    DamageType = DamageType.Explosion,
                    Scale = Vector3.one,
                    RoleType = RoleType.Scp049
                }
            },
            Targets = new List<SchematicConfiguration.TargetConfiguration>
            {
                new SchematicConfiguration.TargetConfiguration
                {
                    Position = Vector3.right * 6,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    TargetType = SynapseTarget.TargetType.Binary
                }
            },
            WorkStations = new List<SchematicConfiguration.SimpleUpdateConfig>
            {
                new ()
                {
                    Position = Vector3.forward * 6,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one
                }
            },
            OldGrenades = new List<SchematicConfiguration.OldGrenadeConfiguration>
            {
                new SchematicConfiguration.OldGrenadeConfiguration
                {
                    UpdateEveryFrame = true,
                    Position = Vector3.left * 10,
                }
            },
            Id = 61,
            Name = "test"
        };
        var schematic = Synapse.Get<SchematicService>().SpawnSchematic(config, context.Player.Position);
    }
}