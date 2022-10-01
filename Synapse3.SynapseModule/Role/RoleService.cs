using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Neuron.Core.Meta;
using Neuron.Modules.Commands.Event;
using Ninject;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Role;

public class RoleService : Service
{
    private readonly IKernel _kernel;
    private readonly SynapseCommandService _command;
    private readonly PlayerService _player;
    private readonly List<RoleAttribute> _customRoles = new();
    private readonly Synapse _synapseModule;

    /// <summary>
    /// The Hightest vanilla number for Roles
    /// </summary>
    public const uint HighestRole = (uint)RoleType.ChaosMarauder;
    
    /// <summary>
    /// A list of all Registered CustomRoles that can spawn
    /// </summary>
    public ReadOnlyCollection<RoleAttribute> CustomRoles => _customRoles.AsReadOnly();

    /// <summary>
    /// Creates a new RoleService
    /// </summary>
    public RoleService(IKernel kernel, SynapseCommandService command, PlayerService player,Synapse synapse)
    {
        _kernel = kernel;
        _command = command;
        _player = player;
        _synapseModule = synapse;
    }

    /// <summary>
    /// This method Enables the RoleService don't call it manually
    /// </summary>
    public override void Enable()
    {
        _command.RemoteAdmin.Subscribe(OnRemoteAdmin);
        
        while (_synapseModule.ModuleRoleBindingQueue.Count != 0)
        {
            var binding = _synapseModule.ModuleRoleBindingQueue.Dequeue();
            LoadBinding(binding);
        }
    }

    /// <summary>
    /// This method Disables the RoleService don't call it manually
    /// </summary>
    public override void Disable()
    {
        _command.RemoteAdmin.Unsubscribe(OnRemoteAdmin);
    }

    /// <summary>
    /// Loads the SynapseRoleBindings
    /// </summary>
    internal void LoadBinding(SynapseRoleBinding binding) => RegisterRole(binding.Info);

    /// <summary>
    /// Returns true if the Id is registered or is an Vanilla Role
    /// </summary>
    public bool IsIdRegistered(uint id)
    {
        if (id is >= 0 and <= HighestRole) return true;

        return _customRoles.Any(x => x.Id == id);
    }
    
    /// <summary>
    /// Returns the Name of an Custom or Vanilla Role
    /// </summary>
    public string GetRoleName(uint id)
    {
        if (id is >= 0 and <= HighestRole)
            return ((RoleType)id).ToString();

        return !IsIdRegistered(id) ? string.Empty : _customRoles.FirstOrDefault(x => x.Id == id)?.Name;
    }

    /// <summary>
    /// Creates a new Instance of the Object to get the Role Name and id and register it. The RoleType must have an RoleInformation attribute
    /// </summary>
    public bool RegisterRole<TRole>() where TRole : ISynapseRole
    {
        var info = typeof(TRole).GetCustomAttribute<RoleAttribute>();
        if (info == null) return false;
        info.RoleScript = typeof(TRole);

        return RegisterRole(info);
    }

    /// <summary>
    /// Register a CustomRole that can be spawned later
    /// </summary>
    public bool RegisterRole(RoleAttribute info)
    {
        if (info.RoleScript == null) return false;
        if (info.Id is >= 0 and <= HighestRole) return false;
        if (IsIdRegistered(info.Id)) return false;

        _customRoles.Add(info);
        return true;
    }

    /// <summary>
    /// Removes the CustomRole from the list of possible roles that can be spawned by Synapse
    /// </summary>
    /// <param name="id">The Id of the Custom Role</param>
    /// <returns>true, when a role was found and could be removed</returns>
    public bool UnRegisterRole(uint id)
    {
        var role = _customRoles.FirstOrDefault(x => x.Id == id);
        return role != null && _customRoles.Remove(role);
    }
    
    /// <inheritdoc cref="GetRole(RoleAttribute)"/>
    public ISynapseRole GetRole(string name)
    {
        var info = CustomRoles.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        return info == null ? null : GetRole(info);
    }

    /// <inheritdoc cref="GetRole(RoleAttribute)"/>
    public ISynapseRole GetRole(uint id)
    {
        var info = CustomRoles.FirstOrDefault(x => x.Id == id);

        return info == null ? null : GetRole(info);
    }

    /// <summary>
    /// Creates a new Instance of a CustomRole
    /// </summary>
    private ISynapseRole GetRole(RoleAttribute info)
    {
        if (info.RoleScript == null) return null;
        var role = (ISynapseRole)_kernel.Get(info.RoleScript);
        role.Attribute = info;
        role.Load();
        return role;
    }

    /// <summary>
    /// This is just to remove the Custom Role when someone is forced to another role
    /// </summary>
    private void OnRemoteAdmin(CommandEvent ev)
    {
        var kill = string.Equals(ev.Context.Command, "kill", StringComparison.OrdinalIgnoreCase);

        if (!string.Equals(ev.Context.Command, "overwatch", StringComparison.OrdinalIgnoreCase) &&
            !kill &&
            !string.Equals(ev.Context.Command, "forceclass", StringComparison.OrdinalIgnoreCase)) return;
        
        if(ev.Context.Arguments.Length == 0) return;

        var ids = ev.Context.Arguments[0].Split('.');
        foreach (var id in ids)
        {
            if (string.IsNullOrEmpty(id))
                continue;
            
            if(!int.TryParse(id,out var result)) continue;

            var player = _player.GetPlayer(result);
            if (player == null) continue;

            player.RemoveCustomRole(kill? DeSpawnReason.Death : DeSpawnReason.ForceClass);
        }
    }
}