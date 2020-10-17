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

                case "SETGROUP":
                    if (!context.Player.HasPermission("synapse.permission.setgroup"))
                    {
                        result.Message = "You don´t have Permission to set the Group of a player (RequiredPermission: synapse.permission.setgroup)";
                        result.State = CommandResultState.NoPermission;
                        break;
                    }

                    var group2 = Server.Get.PermissionHandler.GetServerGroup(context.Arguments.First());
                    if(group2 == null)
                    {
                        result.Message = "No Group with that name was found!";
                        result.State = CommandResultState.Error;
                        break;
                    }

                    if (!context.Player.HasPermission($"synapse.permission.group.{context.Arguments.First().ToLower()}"))
                    {
                        result.Message = $"You don´t have Permission to set a player this specific group (RequiredPermission: synapse.permission.group.{context.Arguments.First().ToLower()})";
                        result.State = CommandResultState.NoPermission;
                        break;
                    }

                    if (context.Arguments.Count < 2)
                        context.Arguments = new System.ArraySegment<string>(new string[] { "setgroup","" });

                    if (context.Arguments.Count < 3)
                        context.Arguments = new System.ArraySegment<string>(new string[] { "setgroup", context.Arguments.ElementAt(1), "" });

                    var player = Server.Get.GetPlayer(context.Arguments.ElementAt(1));
                    var group3 = Server.Get.PermissionHandler.GetServerGroup(context.Arguments.ElementAt(2));

                    if(player == null)
                    {
                        result.Message = "No Player with that Name/ID was found!";
                        result.State = CommandResultState.Error;
                        break;
                    }

                    if (group3 == null)
                    {
                        result.Message = "No Group with that Name was found!";
                        result.State = CommandResultState.Error;
                        break;
                    }

                    player.SynapseGroup = group3;
                    result.Message = "Group of the Player was set!";
                    result.State = CommandResultState.Ok;
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
                        "\nPermission me - Gives you informations ybout your Role" +
                        "\nPermission setgroup player groupname - Sets the Group of a Player for one Round" +
                        "\nPermission groups - Gives you a List of All Groups";
                    result.State = CommandResultState.Ok;
                    break;
            }

            return result;
        }
    }
}
