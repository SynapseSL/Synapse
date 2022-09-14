using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Map.Rooms;

namespace Synapse3.SynapseModule.Role;

public class AbstractRoleAttribute : RoleAttribute, IAbstractRoleConfig
{
    public RoleType Role { get; set; } = RoleType.ClassD;
    public uint EscapeRole { get; set; } = 0;
    public float Health { get; set; } = 100;
    public float MaxHealth { get; set; } = 100;
    public float ArtificialHealth { get; set; } = 0;
    public float MaxArtificialHealth { get; set; } = 75;
    public RoomPoint[] PossibleSpawns { get; set; }
    public SerializedPlayerInventory[] PossibleInventories { get; set; }
    public byte UnitId { get; set; } = 0;
    public string Unit { get; set; } = "";
}