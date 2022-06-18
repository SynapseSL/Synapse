using Synapse3.SynapseModule.CustomRole;
using Synapse3.SynapseModule.Enums;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    /// <summary>
    /// Property for storing LiteAttribute between Patches
    /// </summary>
    internal bool LiteRoleSet { get; set; } = false;
    
    /// <summary>
    /// Changes the role of the player without changing other values
    /// </summary>
    public void ChangeRoleAtPosition(RoleType role)
    {
        LiteRoleSet = true;
        Hub.characterClassManager.SetClassIDAdv(role, true, CharacterClassManager.SpawnReason.ForceClass);
        LiteRoleSet = false;
    }
    
    public SynapseRole CustomRole { get; set; }

    public void RemoveCustomRole(DespawnReason reason)
    {
        
    }
}