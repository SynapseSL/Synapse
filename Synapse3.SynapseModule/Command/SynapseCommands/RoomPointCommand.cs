using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using RemoteAdmin.Communication;
using Synapse3.SynapseModule.Map.Rooms;
using UnityEngine;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseRaCommand(
    CommandName = "RoomPoint",
    Aliases = new[] { "GetRoomPoint", "RP" },
    Parameters = new[] { "(view)" },
    Description = "A command to get the current location as RoomPoint",
    Permission = "synapse.command.mappoint",
    Platforms = new [] { CommandPlatform.RemoteAdmin }
)]
public class RoomPointCommand : SynapseCommand
{
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        RoomPoint point;
        if (context.Arguments.Length != 0)
        {
            point = new RoomPoint(context.Player.Position, context.Player.Rotation);
        }
        else
        {
            var cameraTransform = context.Player.CameraReference.transform;
            Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit raycastHit, 100f);
            point = new RoomPoint(raycastHit.point + Vector3.up * 0.1f, Quaternion.identity);
        }

        result.Response = "\nThe position you are looking at as RoomPoint (change , to . in the syml config):" +
                          $"\n  room: {point.roomName}" +
                          $"\n  x: {point.position.X}" +
                          $"\n  y: {point.position.Y}" +
                          $"\n  z: {point.position.Z}";
                          result.StatusCode = CommandStatusCode.Ok;

        RaClipboard.Send(context.Player.CommandSender, RaClipboard.RaClipBoardType.PlayerId,
            $"{point.roomName} {point.position.X} {point.position.Y} {point.position.Z}");
    }
}