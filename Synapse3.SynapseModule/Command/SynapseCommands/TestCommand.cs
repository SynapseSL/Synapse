using System.Collections.Generic;
using Neuron.Modules.Commands;
using Synapse3.SynapseModule.Enums;
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
    private RoomPoint point = null;
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        result.Response = "Spawn Schematic!";

        var pos = Vector3.zero;
        var rot = Quaternion.identity;
        if (point == null)
        {
            pos = context.Player.Position;
            rot = context.Player.Rotation;

            point = new RoomPoint(pos, rot);
        }
        else
        {
            pos = point.GetMapPosition();
            rot = point.GetMapRotation();
        }

        var door = new SynapseDoor(SynapseDoor.SpawnableDoorType.Ez, pos, rot, Vector3.one);

        context.Player.Position = pos;
    }
}