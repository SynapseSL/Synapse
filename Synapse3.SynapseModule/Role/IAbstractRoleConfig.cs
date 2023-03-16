using System;
using PlayerRoles;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Map.Rooms;

namespace Synapse3.SynapseModule.Role;

public interface IAbstractRoleConfig
{
    public RoleTypeId Role { get; }
    public RoleTypeId VisibleRole { get; }
    public RoleTypeId OwnRole { get; }
    public uint EscapeRole { get; }
    public float Health { get; }
    public float MaxHealth { get; }
    public float ArtificialHealth { get; }
    public float MaxArtificialHealth { get; }
    public RoomPoint[] PossibleSpawns { get; }
    public SerializedPlayerInventory[] PossibleInventories { get; }
    public bool CustomDisplay { get; }
    public bool Hierarchy { get; }
    public SerializedVector3 Scale { get; }
}