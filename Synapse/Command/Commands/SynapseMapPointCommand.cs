using Synapse.Api;
using UnityEngine;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "MapPoint",
        Aliases = new[] { "GetMapPoint", "MP" },
        Description = "A command to get the current location as MapPoint",
        Permission = "synapse.command.mappoint",
        Platforms = new[] { Platform.RemoteAdmin },
        Usage = "Just use the command for the Point"
        )]
    public class SynapseMapPointCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            MapPoint point;
            if (context.Arguments.Count == 1)
            {
                var posParts = context.Arguments.At(0).Split(':');
                var global = new Vector3(
                    float.Parse(posParts[0]),
                    float.Parse(posParts[1]),
                    float.Parse(posParts[2])
                );
                point = new MapPoint(context.Player.Room, global);
            }
            else
            {
                Physics.Raycast(context.Player.CameraReference.transform.position, context.Player.CameraReference.transform.forward, out RaycastHit raycastthit, 100f);
                point = new MapPoint(context.Player.Room, raycastthit.point + (Vector3.up * 0.1f));
            }

            result.Message = "\nThe Position you are looking at as MapPoint (change , to . in the syml config):" +
                $"\n  room: {point.Room.RoomName}" +
                $"\n  x: {point.RelativePosition.x}" +
                $"\n  y: {point.RelativePosition.y}" +
                $"\n  z: {point.RelativePosition.z}";
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}