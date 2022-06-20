using Synapse.Api.Roles;
using System.Linq;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "Setclass",
        Aliases = new[] { "sc", "class" },
        Description = "A command to set the class of a Player",
        Permission = "synapse.command.setclass",
        Platforms = new[] { Platform.RemoteAdmin, Platform.ServerConsole },
        Usage = "setclass player RoleID",
        Arguments = new[] { "Player", "RoleID" }
        )]
    public class SynapseSetClassCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if (context.Arguments.Count < 2)
            {
                result.Message = "Missing Parameters! Command Usage: setclass player RoleID";
                result.State = CommandResultState.Error;
                return result;
            }

            var player = Server.Get.GetPlayer(context.Arguments.FirstElement());
            if (player is null)
            {
                result.Message = "No Player was found!";
                result.State = CommandResultState.Error;
                return result;
            }

            if (!System.Int32.TryParse(context.Arguments.ElementAt(1), out var id))
            {
                result.Message = "Invalid Parameter for RoleID";
                result.State = CommandResultState.Error;
                return result;
            }

            if (!Server.Get.RoleManager.IsIDRegistered(id))
            {
                result.Message = "No Role with this RoleID was found";
                result.State = CommandResultState.Error;
                return result;
            }

            player.CustomRole = null;

            if (id >= 0 && id <= RoleManager.HighestRole)
                player.RoleType = (RoleType)id;
            else
                player.CustomRole = Server.Get.RoleManager.GetCustomRole(id);

            result.Message = "Player Role was set";
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
