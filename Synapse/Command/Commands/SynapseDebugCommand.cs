using Synapse.Api;

namespace Synapse.Command.Commands
{
    [CommandInformation(
           Name = "Debug",
           Aliases = new[] { "dev" },
           Description = "Debug",
           Usage = "debug",
           Permission = "synapse.debug",
           Platforms = new[] { Platform.ClientConsole, Platform.RemoteAdmin, Platform.ServerConsole }
       )]
    public class SynapseDebugCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            new WorkStation(UnityEngine.Vector3.one, UnityEngine.Vector3.zero, UnityEngine.Vector3.one);
            var msg = 0;
            foreach(var room in Map.Get.Rooms)
            {
                if (room.Doors.Count > 0)
                    msg++;
            }
            return new CommandResult
            {
                Message = msg.ToString(),
                State = CommandResultState.Ok
            };
        }
    }
}
