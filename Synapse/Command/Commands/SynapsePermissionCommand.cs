using System.Linq;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "Permission",
        Aliases = new[] { "pm" },
        Description = "A command to manage the Permission System",
        Usage = "Execute the command without parameters for help",
        Permission = "",
        Platforms = new[] { Platform.RemoteAdmin, Platform.ServerConsole },
        Arguments = new[] { "ME/GROUPS/SETGROUP/DELETE" }
    )]
    public class SynapsePermissionCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if (context.Arguments.Count < 1)
                context.Arguments = new(new string[] { "" });

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

                    result.Message += $"\nInheritance:";
                    foreach (var inherit in group.Inheritance)
                        result.Message += $"\n    - {inherit}";

                    break;

                case "GROUPS":
                    if (!context.Player.HasPermission("synapse.permission.groups"))
                    {
                        result.Message = "You don't have permission to get all groups (synapse.permission.groups)";
                        result.State = CommandResultState.NoPermission;
                        break;
                    }

                    var msg = "All Groups:";
                    foreach (var pair in Server.Get.PermissionHandler.Groups)
                        msg += $"\n{pair.Key} Badge: {pair.Value.Badge}";

                    result.Message = msg;
                    result.State = CommandResultState.Ok;
                    break;

                case "SETGROUP":
                    if (!context.Player.HasPermission("synapse.permission.setgroup"))
                    {
                        result.Message = "You don't have permission to set groups (synapse.permission.setgroup)";
                        result.State = CommandResultState.NoPermission;
                        break;
                    }

                    if (context.Arguments.Count() < 3)
                    {
                        result.Message = "Missing parameters";
                        result.State = CommandResultState.Error;
                        break;
                    }

                    var playerid = context.Arguments.ElementAt(2);

                    if (context.Arguments.ElementAt(1) == "-1")
                    {
                        Server.Get.PermissionHandler.RemovePlayerGroup(playerid);
                        result.Message = $"Removed {playerid} player group.";
                        result.State = CommandResultState.Ok;
                        break;
                    }

                    var setGroup = context.Arguments.ElementAt(1);

                    try
                    {
                        if (Server.Get.PermissionHandler.AddPlayerToGroup(setGroup, playerid))
                        {
                            result.Message = $"Set {playerid} player group to {setGroup}.";
                            result.State = CommandResultState.Ok;
                            break;
                        }

                        result.Message = "Invalid UserID or GroupName";
                        result.State = CommandResultState.Error;
                    }
                    catch
                    {
                        result.Message = "Invalid GroupName";
                        result.State = CommandResultState.Error;
                    }
                    break;

                case "DELETE":
                    if (!context.Player.HasPermission("synapse.permission.delete"))
                    {
                        result.Message = "You don't have permission to delete groups (synapse.permission.delete)";
                        result.State = CommandResultState.NoPermission;
                        break;
                    }

                    if (context.Arguments.Count < 2) return new()
                    {
                        Message = "Missing group name",
                        State = CommandResultState.Error
                    };

                    if (Server.Get.PermissionHandler.DeleteServerGroup(context.Arguments.At(1)))
                        return new()
                        {
                            Message = "Group successfully deleted",
                            State = CommandResultState.Ok
                        };
                    else
                        return new()
                        {
                            Message = "No Group with that Name was found",
                            State = CommandResultState.Error
                        };

                default:
                    result.Message = "All Permission Commands:" +
                        "\nPermission me - Gives you information about your Role" +
                        "\nPermission groups - Gives you a List of All Groups" +
                        "\nPermission setgroup {Group} {UserID} - Sets a User group" +
                        "\nPermission delte {Group}";
                    result.State = CommandResultState.Ok;
                    break;
            }

            return result;
        }
    }
}