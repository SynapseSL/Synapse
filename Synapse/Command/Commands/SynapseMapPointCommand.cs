using Synapse.Api;
using UnityEngine;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "MapPoint",
        Aliases = new [] { "GetMapPoint","MP" },
        Description = "A command to get the current location as MapPoint",
        Permission = "synapse.command.mappoint",
        Platforms = new [] { Platform.RemoteAdmin },
        Usage = "Just use the command for the Point"
        )]
    public class SynapseMapPointCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            Physics.Raycast(context.Player.CameraReference.transform.position, context.Player.CameraReference.transform.forward, out RaycastHit raycastthit, 100f);

            var point = new MapPoint(context.Player.Room, raycastthit.point + (Vector3.up * 0.1f));
            result.Message = "\nThe Position you are looking at as MapPoint:" +
                $"\n  room: {point.Room.RoomName}" +
                $"\n  x: {point.RelativePosition.x.ToString().Replace(",",".")}" +
                $"\n  y: {point.RelativePosition.y.ToString().Replace(",", ".")}" +
                $"\n  z: {point.RelativePosition.z.ToString().Replace(",", ".")}";
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
