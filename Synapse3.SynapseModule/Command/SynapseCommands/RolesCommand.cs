using System;
using System.Linq;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using PlayerRoles;
using Synapse3.SynapseModule.Role;
using Synapse3.SynapseModule.Teams;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseRaCommand(
    CommandName = "Roles",
    Aliases = new[] { "rol, role" },
    Parameters = new[] { "(role ID or Name)" },
    Description = "A command which provides information about roles present in the server",
    Permission = "synapse.command.roles",
    Platforms = new[] { CommandPlatform.RemoteAdmin, CommandPlatform.ServerConsole, CommandPlatform.ServerConsole }
)]
public class RolesCommand : SynapseCommand
{
    private readonly RoleService _role;
    private readonly ServerService _server;
    private readonly TeamService _team;

    public RolesCommand(RoleService role, TeamService team,ServerService server)
    {
        _role = role;
        _server = server;
        _team = team;
    }

    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        if (context.Arguments.Length > 0)
        {
            var arg = string.Join(" ", context.Arguments);

            if (GetInfoCustomRole(arg, ref result))
                return;

            if (GetInfoVanilaRole(arg, ref result))
                return;

            result.Response = "No Role was found";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        var allRoleType = ((RoleTypeId[])Enum.GetValues(typeof(RoleTypeId))).ToList();
        allRoleType.Remove(RoleTypeId.None);
       
        result.Response = "All Roles:";
        foreach (var role in allRoleType)
            result.Response += $"\n{role.ToString()} Id: {(int)role}";
        foreach (var role in _role.CustomRoles.OrderBy(p => p.Id))
            result.Response += $"\n{role.Name} Id: {role.Id}";

        result.StatusCode = CommandStatusCode.Ok;
    }

    private bool GetInfoCustomRole(string arg, ref CommandResult result)
    {
        RoleAttribute role = null;
        if (uint.TryParse(arg, out var id))
        {
            if (_role.IsIdVanila(id)) return false;

            role = _role.CustomRoles.FirstOrDefault(p => p.Id == id);
        }
        else
        {
            role = _role.CustomRoles.FirstOrDefault(p => p.Name == arg);
        }
        
        if (role == null) return false;

        var assmebly = role.RoleScript.Assembly;
        var plugin = _server.Plugins.FirstOrDefault(p => p.PluginType.Assembly.Equals(assmebly));
        var teamName = _team.GetTeamName(role.TeamId);

        result.Response = $"\n{role.Name}" +
                            $"\n    - Id: {role.Id}" +
                            $"\n    - Team: {teamName}" +
                            $"\n    - TeamId: {role.TeamId}" +
                            $"\n    - Plugin: {plugin?.Attribute.Name ?? "Unknow"}";

        result.StatusCode = CommandStatusCode.Ok;
        return true;
    }

    private bool GetInfoVanilaRole(string arg, ref CommandResult result)
    {
        RoleTypeId role = RoleTypeId.None;
        if (uint.TryParse(arg, out var id))
        {
            if (!_role.IsIdVanila(id)) return false;

            role = (RoleTypeId)id;
        }
        else
        {
            if (!Enum.TryParse<RoleTypeId>(arg, out role))
                return false;
        }

        PlayerRoleLoader.TryGetRoleTemplate<PlayerRoleBase>(role, out var playerRole);
        var teamId = (uint)playerRole.Team;
        var teamName = _team.GetTeamName(teamId);
        
        result.Response = $"\n{role.ToString()}" +
                            $"\n    - Id: {(int)role}" +
                            $"\n    - Team: {teamName}" +
                            $"\n    - TeamId: {teamId}";

        result.StatusCode = CommandStatusCode.Ok;
        return true;
    }
}