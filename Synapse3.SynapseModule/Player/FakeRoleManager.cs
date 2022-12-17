using System;
using System.Collections.Generic;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.PlayableScps.Scp049.Zombies;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.PlayableScps.Scp173;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.SpawnData;
using PlayerRoles.Spectating;
using PlayerStatsSystem;
using RelativePositioning;

namespace Synapse3.SynapseModule.Player;

public class FakeRoleManager
{
    private readonly SynapsePlayer _player;
    private readonly PlayerService _playerService;

    internal FakeRoleManager(SynapsePlayer player, MirrorService mirror, PlayerService playerService)
    {
        _player = player;
        _playerService = playerService;
    }

    public void Reset()
    {
        _ownVisibleRole = new RoleInfo(RoleTypeId.None, null, null);
        _visibleRole = new RoleInfo(RoleTypeId.None, null, null);
        ToPlayerVisibleRole.Clear();
        VisibleRoleCondition.Clear();
        UpdateAll();
    }
    
    public void UpdateAll()
    {
        foreach (var player in _playerService.Players)
        {
            UpdatePlayer(player);
        }
    }
    
    public void UpdatePlayer(SynapsePlayer player) => player.SendNetworkMessage(new RoleSyncInfo(_player, player));

    private RoleInfo _ownVisibleRole = new(RoleTypeId.None, null, null);
    public RoleInfo OwnVisibleRole
    {
        get => _ownVisibleRole;
        set
        {
            _ownVisibleRole = value;
            UpdatePlayer(_player);
        }
    }

    private RoleInfo _visibleRole = new(RoleTypeId.None, null, null);
    public RoleInfo VisibleRole
    {
        get => _visibleRole;
        set
        {
            _visibleRole = value;
            foreach (var player in _playerService.Players)
            {
                if (player != _player)
                    UpdatePlayer(player);
            }
        }
    }
    public Dictionary<Func<SynapsePlayer, bool>, RoleInfo> VisibleRoleCondition { get; set; } = new();
    public Dictionary<SynapsePlayer, RoleInfo> ToPlayerVisibleRole { get; set; } = new();

    public void WriteRoleSyncInfoFor(SynapsePlayer receiver, NetworkWriter writer)
    {
        writer.WriteUInt32(_player.NetworkIdentity.netId);
        var roleInfo = GetRoleInfo(receiver);
        writer.WriteRoleType(roleInfo.RoleTypeId);

        if (typeof(IPublicSpawnDataWriter).IsAssignableFrom(EnumToType[roleInfo.RoleTypeId]) &&
            roleInfo.WritePublicSpawnData != null)
            roleInfo.WritePublicSpawnData(writer);

        if (receiver == _player && typeof(IPrivateSpawnDataWriter).IsAssignableFrom(EnumToType[roleInfo.RoleTypeId]) &&
            roleInfo.WritePrivateSpawnData != null)
            roleInfo.WritePrivateSpawnData(writer);
    }

    public RoleInfo GetRoleInfo(SynapsePlayer receiver)
    {
        if (receiver == _player && OwnVisibleRole.RoleTypeId != RoleTypeId.None)
        {
            return OwnVisibleRole;
        }

        if (ToPlayerVisibleRole.ContainsKey(receiver))
        {
            return ToPlayerVisibleRole[receiver];
        }

        foreach (var condition in VisibleRoleCondition)
        {
            if (condition.Key(receiver))
                return condition.Value;
        }

        if (VisibleRole.RoleTypeId != RoleTypeId.None)
        {
            return VisibleRole;
        }

        var publicWriter = _player.CurrentRole as IPublicSpawnDataWriter;
        var privateWriter = _player.CurrentRole as IPrivateSpawnDataWriter;

        return new RoleInfo(_player.CurrentRole.RoleTypeId,
            publicWriter == null ? null : publicWriter.WritePublicSpawnData,
            privateWriter == null ? null : privateWriter.WritePrivateSpawnData);
    }

    public static readonly Dictionary<RoleTypeId, Type> EnumToType = new()
    {
        { RoleTypeId.None, typeof(NoneRole) },
        { RoleTypeId.Scp173, typeof(Scp173Role) },
        { RoleTypeId.ClassD, typeof(HumanRole) },
        { RoleTypeId.Spectator, typeof(SpectatorRole) },
        { RoleTypeId.Scp106, typeof(Scp106Role) },
        { RoleTypeId.NtfSpecialist, typeof(HumanRole) },
        { RoleTypeId.Scp049, typeof(Scp049Role) },
        { RoleTypeId.Scientist, typeof(HumanRole) },
        { RoleTypeId.Scp079, typeof(Scp079Role) },
        { RoleTypeId.ChaosConscript, typeof(HumanRole) },
        { RoleTypeId.Scp0492, typeof(ZombieRole) },
        { RoleTypeId.NtfSergeant, typeof(HumanRole) },
        { RoleTypeId.NtfCaptain, typeof(HumanRole) },
        { RoleTypeId.NtfPrivate, typeof(HumanRole) },
        { RoleTypeId.Tutorial, typeof(HumanRole) },
        { RoleTypeId.FacilityGuard, typeof(HumanRole) },
        { RoleTypeId.Scp939, typeof(Scp939Role) },
        { RoleTypeId.ChaosRifleman, typeof(HumanRole) },
        { RoleTypeId.ChaosRepressor, typeof(HumanRole) },
        { RoleTypeId.ChaosMarauder, typeof(HumanRole) },
        { RoleTypeId.Scp096, typeof(Scp096Role) },
        //TODO:
        { RoleTypeId.CustomRole, typeof(NoneRole) },
        { RoleTypeId.Overwatch, typeof(NoneRole) }
    };
}

public class RoleInfo
{
    public RoleInfo(RoleTypeId role, Action<NetworkWriter> writePublicSpawnData,
        Action<NetworkWriter> writePrivateSpawnData)
    {
        RoleTypeId = role;
        WritePublicSpawnData = writePublicSpawnData;
        WritePrivateSpawnData = writePrivateSpawnData;
    }

    public RoleInfo(RoleTypeId role, SynapsePlayer player)
    {
        RoleTypeId = role;
        switch (role)
        {
            case RoleTypeId.Spectator: PrepareSpectator(); break;
            case RoleTypeId.Scp0492:
                PrepareZombieRole(600, player);
                break;
            
            default:
                if (typeof(HumanRole).IsAssignableFrom(FakeRoleManager.EnumToType[role]))
                    PrepareHumanRole(role, 0, player);
                else if (typeof(FpcStandardRoleBase).IsAssignableFrom(FakeRoleManager.EnumToType[role]))
                    PrepareFpcRole(player);
                break;
        }
    }

    public void PrepareZombieRole(ushort maxHealth, SynapsePlayer playerToShow)
    {
        RoleTypeId = RoleTypeId.Scp0492;
        WritePublicSpawnData = writer =>
        {
            writer.WriteUInt16(maxHealth);
            writer.WriteRelativePosition(new RelativePosition(playerToShow.Position));

            if (playerToShow.CurrentRole is FpcStandardRoleBase role)
            {
                role.FpcModule.MouseLook.GetSyncValues(0, out var rotation, out _);
                writer.WriteUInt16(rotation);
            }
            else
            {
                writer.WriteUInt16(0);
            }
        };
    }

    public void PrepareHumanRole(RoleTypeId humanRole, byte unitId, SynapsePlayer playerToShow)
    {
        if (!typeof(HumanRole).IsAssignableFrom(FakeRoleManager.EnumToType[humanRole])) return;
        RoleTypeId = humanRole;
        WritePublicSpawnData = writer =>
        {
            if (humanRole is RoleTypeId.FacilityGuard or RoleTypeId.NtfCaptain or RoleTypeId.NtfPrivate
                or RoleTypeId.NtfSergeant or RoleTypeId.NtfSpecialist)
                writer.WriteByte(unitId);
            
            writer.WriteRelativePosition(new RelativePosition(playerToShow.Position));
            
            if (playerToShow.CurrentRole is FpcStandardRoleBase role)
            {
                role.FpcModule.MouseLook.GetSyncValues(0, out var rotation, out _);
                writer.WriteUInt16(rotation);
            }
            else
            {
                writer.WriteUInt16(0);
            }
        };
    }

    public void PrepareFpcRole(SynapsePlayer playerToShow)
    {
        WritePublicSpawnData = writer =>
        {
            writer.WriteRelativePosition(new RelativePosition(playerToShow.Position));
            
            if (playerToShow.CurrentRole is FpcStandardRoleBase role)
            {
                role.FpcModule.MouseLook.GetSyncValues(0, out var rotation, out _);
                writer.WriteUInt16(rotation);
            }
            else
            {
                writer.WriteUInt16(0);
            }
        };
    }

    public void PrepareSpectator(DamageHandlerBase damageHandler = null)
    {
        RoleTypeId = RoleTypeId.Spectator;
        WritePrivateSpawnData = writer =>
        {
            if (damageHandler == null)
                writer.WriteSpawnReason(SpectatorSpawnReason.None);
            else 
                damageHandler.WriteDeathScreen(writer);
        };
    }
    
    public RoleTypeId RoleTypeId { get; set; }
    
    public Action<NetworkWriter> WritePublicSpawnData { get; set; }
    public Action<NetworkWriter> WritePrivateSpawnData { get; set; }
}