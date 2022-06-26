using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Neuron.Core.Meta;
using Neuron.Modules.Commands.Event;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Role;

public class RoleService : Service
{
    private readonly SynapseCommandService _command;
    private readonly PlayerService _player;
    private List<RoleInformation> _customRoles = new();

    public const int HighestRole = (int)RoleType.ChaosMarauder;
    
    /// <summary>
    /// A list of all Registered CustomRoles that can spawn
    /// </summary>
    public ReadOnlyCollection<RoleInformation> CustomRoles => _customRoles.AsReadOnly();

    /// <summary>
    /// Creates a new RoleService
    /// </summary>
    public RoleService(SynapseCommandService command, PlayerService player)
    {
        _command = command;
        _player = player;
    }

    /// <summary>
    /// This method Enables the RoleService don't call it manually
    /// </summary>
    public override void Enable()
    {
        _command.RemoteAdmin.Subscribe(OnRemoteAdmin);
    }

    /// <summary>
    /// Returns true if the Id is registered or is an Vanilla Role
    /// </summary>
    public bool IsIdRegistered(int id)
    {
        if (id is >= -1 and <= HighestRole) return true;

        if (_customRoles.Any(x => x.ID == id)) return true;

        return false;
    }
    
    /// <summary>
    /// Returns the Name of an Custom or Vanilla Role
    /// </summary>
    public string GetRoleName(int id)
    {
        if (id is >= -1 and <= HighestRole)
            return ((RoleType)id).ToString();

        if (!IsIdRegistered(id)) return string.Empty;

        return _customRoles.FirstOrDefault(x => x.ID == id)?.Name;
    }

    /// <summary>
    /// Creates a new Instance of the Object to get the Role Name & id and register it. The Role must have a empty constructor for this
    /// </summary>
    public bool RegisterRole<TRole>() where TRole : ISynapseRole
    {
        var role = (ISynapseRole)Activator.CreateInstance(typeof(TRole));
        var info = new RoleInformation(role.GetRoleName(), role.GetRoleID(), typeof(TRole));

        return RegisterRole(info);
    }

    /// <summary>
    /// Register a CustomRole that can be spawned later
    /// </summary>
    public bool RegisterRole(RoleInformation info)
    {
        if (info.ID is >= -1 and <= HighestRole) return false;
        if (IsIdRegistered(info.ID)) return false;

        _customRoles.Add(info);
        return true;
    }

    /// <summary>
    /// Removes the CustomRole from the list of possible roles that can be spawned by Synapse
    /// </summary>
    /// <param name="id">The Id of the Custom Role</param>
    /// <returns>true, when a role was found and could be removed</returns>
    public bool UnRegisterRole(int id)
    {
        var role = _customRoles.FirstOrDefault(x => x.ID == id);
        if (role != null)
            return _customRoles.Remove(role);

        return false;
    }
    
    /// <inheritdoc cref="GetRole(Synapse3.SynapseModule.Role.RoleInformation)"/>
    public ISynapseRole GetRole(string name)
    {
        var info = CustomRoles.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (info == null)
            return null;

        return GetRole(info);
    }

    /// <inheritdoc cref="GetRole(Synapse3.SynapseModule.Role.RoleInformation)"/>
    public ISynapseRole GetRole(int id)
    {
        var info = CustomRoles.FirstOrDefault(x => x.ID == id);

        if (info == null) 
            return null;

        return GetRole(info);
    }

    /// <summary>
    /// Creates a new Instance of a CustomRole
    /// </summary>
    private ISynapseRole GetRole(RoleInformation info)
    {
        if (info.RoleScript.GetConstructors().Any(x => x.GetParameters().Count() == 1 && x.GetParameters().First().ParameterType == typeof(int)))
            return (ISynapseRole)Activator.CreateInstance(info.RoleScript, new object[] { info.ID });

        return (ISynapseRole)Activator.CreateInstance(info.RoleScript);
    }

    /// <summary>
    /// This is just to remove the Custom Role when someone is forced to another role
    /// </summary>
    private void OnRemoteAdmin(CommandEvent ev)
    {
        var kill = string.Equals(ev.Context.Command, "kill", StringComparison.OrdinalIgnoreCase);
        
        if(!string.Equals(ev.Context.Command,"overwatch",StringComparison.OrdinalIgnoreCase) &&
           !kill &&
           !string.Equals(ev.Context.Command,"forceclass",StringComparison.OrdinalIgnoreCase)) return;
        
        if(ev.Context.Arguments.Length == 0) return;

        var ids = ev.Context.Arguments[0].Split('.');
        foreach (var id in ids)
        {
            if (string.IsNullOrEmpty(id))
                continue;
            
            if(!int.TryParse(id,out var result)) continue;

            var player = _player.GetPlayer(result);
            if (player == null) continue;

            player.RemoveCustomRole(kill? DespawnReason.Death : DespawnReason.Forceclass);
        }
    }
}