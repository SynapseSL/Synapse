using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using Synapse3.SynapseModule.Permissions;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseRaCommand(
    CommandName = "Permission",
    Aliases = new[] {"pm", "perm", "perms", "permissions"},
    Description = "A command to manage the permission system",
    Permission = "",
    Platforms = new[] { CommandPlatform.ServerConsole, CommandPlatform.RemoteAdmin }
)]
public class PermissionCommand : SynapseCommand
{
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        if (context.Arguments.Length < 1) context.Arguments = new[] { "" };
        var permissionService = Synapse.Get<PermissionService>();

        switch (context.Arguments[0].ToUpper())
        {
            case "ME":
                var group = context.Player.SynapseGroup;
                result.Response = "\nYour Group:" +
                                  $"\nDefault: {group.Default}" +
                                  $"\nNorthWood: {group.NorthWood}" +
                                  $"\nRemoteAdmin: {group.RemoteAdmin}" +
                                  $"\nBadge: {group.Badge}" +
                                  $"\nColor: {group.Color}" +
                                  $"\nCover: {group.Cover}" +
                                  $"\nHidden: {group.Hidden}" +
                                  $"\nKickPower: {group.KickPower}" +
                                  $"\nRequiredKickPower: {group.RequiredKickPower}" +
                                  "\nPermissions:";

                foreach (var perm in group.Permissions)
                    result.Response += $"\n    - {perm}";

                result.Response += "\nInheritance:";
                foreach (var inherit in group.Inheritance)
                    result.Response += $"\n    - {inherit}";

                break;

            case "GROUPS":
                if (!context.Player.HasPermission("synapse.permission.groups"))
                {
                    result.Response = "You don't have permission to get all groups (synapse.permission.groups)";
                    result.StatusCode = CommandStatusCode.Forbidden;
                    break;
                }

                var msg = "All Groups:";
                foreach (var pair in permissionService.Groups)
                    msg += $"\n{pair.Key} Badge: {pair.Value.Badge}";

                result.Response = msg;
                break;

            case "SETGROUP":
                if (!context.Player.HasPermission("synapse.permission.setgroup"))
                {
                    result.Response = "You don't have permission to set groups (synapse.permission.setgroup)";
                    result.StatusCode = CommandStatusCode.Forbidden;
                    break;
                }

                if (context.Arguments.Length < 3)
                {
                    result.Response = "Missing parameters";
                    result.StatusCode = CommandStatusCode.BadSyntax;
                    break;
                }

                var playerid = context.Arguments[2];

                if (context.Arguments[1] == "-1")
                {
                    permissionService.RemovePlayerGroup(playerid);
                    result.Response = $"Removed {playerid} player group.";
                    break;
                }

                var setGroup = context.Arguments[1];
                try
                {
                    if (permissionService.AddPlayerToGroup(setGroup, playerid))
                    {
                        result.Response = $"Set {playerid} player group to {setGroup}.";
                        break;
                    }

                    result.Response = "Invalid UserID or GroupName";
                    result.StatusCode = CommandStatusCode.Error;
                }
                catch
                {
                    result.Response = "Invalid GroupName";
                    result.StatusCode = CommandStatusCode.Error;
                }

                break;

            case "DELETE":
                if (!context.Player.HasPermission("synapse.permission.delete"))
                {
                    result.Response = "You don't have permission to delete groups (synapse.permission.delete)";
                    result.StatusCode = CommandStatusCode.Forbidden;
                    break;
                }

                if (context.Arguments.Length < 2)
                {
                    result.Response = "Missing group name";
                    result.StatusCode = CommandStatusCode.Error;
                }

                if (permissionService.DeleteServerGroup(context.Arguments[1]))
                {
                    result.Response = "Group successfully deleted";
                }
                else
                {
                    result.Response = "No Group with that Name was found";
                    result.StatusCode = CommandStatusCode.Error;
                }
                break;

            default:
                result.Response = "All Permission Commands:" +
                                  "\nPermission me - Gives you information about your Role" +
                                  "\nPermission groups - Gives you a List of All Groups" +
                                  "\nPermission setgroup {Group} {UserID} - Sets a User group" +
                                  "\nPermission delete {Group}";
                break;
        }
    }
}