using Synapse.Api;
using UnityEngine;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "MapPoint",
        Aliases = new [] { "GetMapPoint","MP" },
        Description = "A Command to get your Current Location as MapPoint",
        Permission = "synapse.command.mappoint",
        Platforms = new [] { Platform.RemoteAdmin },
        Usage = "Just use the Command and you get the Point"
        )]
    public class SynapseMapPointCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();
            if (!context.Player.HasPermission("synapse.command.mappoint"))
            {
                result.Message = "You dont have permissions to execute this command";
                result.State = CommandResultState.NoPermission;
                return result;
            }

            Physics.Raycast(context.Player.CameraReference.transform.position, context.Player.CameraReference.transform.forward, out RaycastHit raycastthit, 100f);

            var point = new MapPoint(context.Player.Room, raycastthit.point + (Vector3.up * 0.1f));
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
