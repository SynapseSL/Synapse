using System.Collections.Generic;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Role;

public interface ISynapseRole
{
    SynapsePlayer Player { get; set; }

    RoleAttribute Attribute { get; set; }

    void Load();

    List<uint> GetFriendsID();

    List<uint> GetEnemiesID();

    void TryEscape();

    void SpawnPlayer(bool spawnLite);

    void DeSpawn(DeSpawnReason reason);
}

public interface IUpdateDisplayRole : ISynapseRole
{
    void UpdateDisplayName(UpdateDisplayNameEvent ev);
}

public interface IHierarchizedRole : ISynapseRole
{
    float HierachiPower { get; }
}

public interface IAbstractRoleConfig
{
    public RoleType Role { get; }
    public RoleType VisibleRole { get; }
    public uint EscapeRole { get; }
    public float Health { get; }
    public float MaxHealth { get; }
    public float ArtificialHealth { get; }
    public float MaxArtificialHealth { get; }
    public RoomPoint[] PossibleSpawns { get; }
    public SerializedPlayerInventory[] PossibleInventories { get; }
    public byte UnitId { get; }
    public string Unit { get; }
    public SerializedVector3 Scale { get; }
}

