using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Role;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    /// <summary>
    /// Changes the role of the player without changing other values
    /// </summary>
    public void ChangeRoleLite(RoleType role)
    {
        ClassManager.CurClass = role;
        FakeRoleManager.UpdateAll();
    }

    private ISynapseRole _customRole;

    /// <summary>
    /// The Current CustomRole of the Player. Is null when he is just a Vanilla Role
    /// </summary>
    public ISynapseRole CustomRole
    {
        get => _customRole;
        set
        {
            RemoveCustomRole(DeSpawnReason.API);
            
            if(value is null)
                return;
            
            _customRole = value;
            _customRole.Player = this;
            _customRole.SpawnPlayer(false);
            _playerEvents.ChangeRole.Raise(new ChangeRoleEvent(this) { RoleId = value.Attribute.Id });
        }
    }
    
    /// <summary>
    /// Removes the CustomRole Of the Player if he has one
    /// </summary>
    public void RemoveCustomRole(DeSpawnReason reason)
    {
        var storedRole = _customRole;
        _customRole = null;
        storedRole?.DeSpawn(reason);
    }

    /// <inheritdoc cref="SpawnCustomRole(ISynapseRole,bool)"/>
    public void SpawnCustomRole(uint id, bool liteSpawn = false)
        => SpawnCustomRole(_role.GetRole(id), liteSpawn);
    
    /// <summary>
    /// Spawns the Player with that CustomRole
    /// </summary>
    public void SpawnCustomRole(ISynapseRole role, bool liteSpawn = false)
    {
        if(role is null)
            return;
        
        RemoveCustomRole(DeSpawnReason.API);

        _customRole = role;
        _customRole.Player = this;
        _customRole.SpawnPlayer(liteSpawn);
    }

    /// <summary>
    /// The Current RoleID of the Player. Combines RoleType and CustomRole
    /// </summary>
    public uint RoleID
    {
        get
        {
            if (CustomRole == null) return RoleType == RoleType.None ? RoleService.NoneRole : (uint)RoleType;
            return CustomRole.Attribute.Id;
        }
        set
        {
            if (value is >= 0 and <= (int)RoleService.HighestRole)
            {
                RemoveCustomRole(DeSpawnReason.API);
                RoleType = (RoleType)value;
                return;
            }
            if(!_role.IsIdRegistered(value)) return;

            CustomRole = _role.GetRole(value);
        }
    }

    /// <summary>
    /// The Name of the Role the player currently has
    /// </summary>
    public string RoleName
    {
        get
        {
            if (CustomRole == null) return RoleType.ToString();
            return CustomRole.Attribute.Name;
        }
    }

    public string TeamName => _team.GetTeamName(TeamID);
}