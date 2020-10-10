using System.Linq;

namespace Synapse.Command.Commands
{
    [CommandInformations(
        Name = "permission",
        Aliases = new[] { "pm" },
        Description = "A Command for managing the Permission System",
        Usage = "Use \"permission help\"",
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
                        return result;
                    }

                    var group2 = Server.Get.PermissionHandler.GetServerGroup(context.Arguments.First());
                    if(group2 == null)
                    {
                        result.Message = "No Group with that name was found!";
                        result.State = CommandResultState.Error;
                        return result;
                    }

                    if (!context.Player.HasPermission($"synapse.permission.group.{context.Arguments.First().ToLower()}"))
                    {
                        result.Message = $"You don´t have Permission to set a player this specific group (RequiredPermission: synapse.permission.group.{context.Arguments.First().ToLower()})";
                        result.State = CommandResultState.NoPermission;
                        return result;
                    }


                    break;

                case "ADDPERMISSION":
                    break;

                default:
                    break;
            }

            return result;
        }
    }
}
