using Synapse3.SynapseModule.Role;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    /// <summary>
    /// Property for storing LiteAttribute between Patches
    /// </summary>
    internal bool LiteRoleSet { get; set; }
    
    /// <summary>
    /// Changes the role of the player without changing other values
    /// </summary>
    public void ChangeRoleLite(RoleType role)
    {
        LiteRoleSet = true;
        Hub.characterClassManager.SetClassIDAdv(role, true, CharacterClassManager.SpawnReason.None);
        LiteRoleSet = false;
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
            RemoveCustomRole(DespawnReason.API);
            
            if(value is null)
                return;
            
            _customRole = value;
            _customRole.Player = this;
            _customRole.SpawnPlayer(false);
        }
    }
    
    /// <summary>
    /// Removes the CustomRole Of the Player if he has one
    /// </summary>
    public void RemoveCustomRole(DespawnReason reason)
    {
        _customRole?.DeSpawn(reason);
        _customRole = null;
    }

    /// <inheritdoc cref="SpawnCustomRole(ISynapseRole,bool)"/>
    public void SpawnCustomRole(int id, bool liteSpawn = false)
        => SpawnCustomRole(Synapse.Get<RoleService>().GetRole(id), liteSpawn);
    
    /// <summary>
    /// Spawns the Player with that CustomRole
    /// </summary>
    public void SpawnCustomRole(ISynapseRole role, bool liteSpawn = false)
    {
        if(role is null)
            return;
        
        RemoveCustomRole(DespawnReason.API);

        _customRole = role;
        _customRole.Player = this;
        _customRole.SpawnPlayer(liteSpawn);
    }

    /// <summary>
    /// The Current RoleID of the Player. Combines RoleType and CustomRole
    /// </summary>
    public int RoleID
    {
        get
        {
            if (CustomRole == null) return (int)RoleType;
            return CustomRole.Attribute.ID;
        }
        set
        {
            if (value is >= -1 and <= (int)RoleService.HighestRole)
            {
                RemoveCustomRole(DespawnReason.API);
                RoleType = (RoleType)value;
                return;
            }

            var service = Synapse.Get<RoleService>();
            if(!service.IsIdRegistered(value)) return;

            CustomRole = service.GetRole(value);
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
}