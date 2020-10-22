using System.Linq;

namespace Synapse.Command.Commands
{
    [CommandInformations(
        Name = "permission",
        Aliases = new[] { "pm" },
        Description = "A Command for managing the Permission System",
        Usage = "Execute the command without paramater to get a Help",
        Permission = "none",
        Platforms = new[] { Platform.RemoteAdmin,Platform.ServerConsole }
    )]
    public class SynapsePermissionCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if(context.Arguments.Count < 1)
                context.Arguments = new System.ArraySegment<string>(new string[] { "" });

            switch (context.Arguments.First().ToUpper())
            {
                case "ME":
                    var group = context.Player.SynapseGroup;
                    result.Message = "\nYour Group:" +
                        $"\nDefault: {group.Default}" +
                        $"\nNorthWood: {group.Northwood}" +
                        $"\nRemoteAdmin: {group.RemoteAdmin}" +
                        $"\nBadge: {group.Badge}" +
                        $"\nColor: {group.Color}" +
                        $"\nCover: {group.Cover}" +
                        $"\nHidden: {group.Hidden}" +
                        $"\nKickPower: {group.KickPower}" +
                        $"\nRequiredKickPower: {group.RequiredKickPower}" +
                        $"\nPermissions:";
                    foreach (var perm in group.Permissions)
                        result.Message += $"\n    - {perm}";
                    break;

                case "GROUPS":
                    if(!context.Player.HasPermission("synapse.permission.groups"))
                    {
                        result.Message = "You don´t have Permission to get all groups (synapse.permission.groups)";
                        result.State = CommandResultState.Error;
                        break;
                    }

                    var msg = "All Groups:";
                    foreach (var pair in Server.Get.PermissionHandler.Groups)
                        msg += $"\n{pair.Key} Badge: {pair.Value.Badge}";

                    result.Message = msg;
                    result.State = CommandResultState.Ok;
                    break;

                default:
                    result.Message = "All Permission Commands:" +
                        "\nPermission me - Gives you informations about your Role" +
                        "\nPermission groups - Gives you a List of All Groups";
                    result.State = CommandResultState.Ok;
                    break;
            }

            return result;
        }
    }
}
