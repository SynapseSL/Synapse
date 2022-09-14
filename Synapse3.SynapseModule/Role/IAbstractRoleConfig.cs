using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Map.Rooms;

namespace Synapse3.SynapseModule.Role;

public interface IAbstractRoleConfig
{
    public RoleType Role { get; }
    public uint EscapeRole { get; }
    public float Health { get; }
    public float MaxHealth { get; }
    public float ArtificialHealth { get; }
    public float MaxArtificialHealth { get; }
    public RoomPoint[] PossibleSpawns { get; }
    public SerializedPlayerInventory[] PossibleInventories { get; }
    public byte UnitId { get; }
    public string Unit { get; }
}