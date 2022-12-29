using System;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp049.Zombies;
using PlayerRoles.SpawnData;
using RelativePositioning;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Role;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    public FakeRoleManager FakeRoleManager { get; }
    
    public PlayerRoleBase CurrentRole
    {
        get => Hub.roleManager.CurrentRole;
        set => Hub.roleManager.CurrentRole = value;
    }
    
    /// <summary>
    /// The Current RoleType of the Player. Use RoleID instead if you want to set the Role of the Player and remove potentially active custom roles
    /// </summary>
    public virtual RoleTypeId RoleType
    {
        get => CurrentRole.RoleTypeId;
        set => Hub.roleManager.ServerSetRole(value, RoleChangeReason.None);
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
            var prevRole = _customRole;
            RemoveCustomRole(DeSpawnReason.API);
            
            if(value is null)
                return;
            
            _customRole = value;
            _customRole.Player = this;
            _customRole.SpawnPlayer(prevRole, false);
            _playerEvents.ChangeRole.Raise(new ChangeRoleEvent(this) { RoleId = value.Attribute.Id });
        }
    }

    public bool HasCustomRole => CustomRole != null;
    
    /// <summary>
    /// The Current RoleID of the Player. Combines RoleType and CustomRole
    /// </summary>
    public uint RoleID
    {
        get
        {
            if (CustomRole == null) return RoleType == RoleTypeId.None ? RoleService.NoneRole : (uint)RoleType;
            return CustomRole.Attribute.Id;
        }
        set
        {
            if (value is >= 0 and <= (int)RoleService.HighestRole)
            {
                RemoveCustomRole(DeSpawnReason.API);
                RoleType = (RoleTypeId)value;
                return;
            }
            if(!_role.IsIdRegistered(value)) return;

            CustomRole = _role.GetRole(value);
        }
    }

    /// <summary>
    /// Changes the role of the player without changing other values
    /// </summary>
    public void ChangeRoleLite(RoleTypeId role, NetworkReader data = null)
    {
        PlayerRoleBase prevRole = null;
        if (RoleManager._anySet)
        {
            prevRole = RoleManager.CurrentRole;
            prevRole.DisableRole(role);
        }
        var newRole = RoleManager.GetRoleBase(role);
        var newRoleTransform = newRole.transform;
        newRoleTransform.parent = transform;
        newRoleTransform.localPosition = Vector3.zero;
        newRoleTransform.localRotation = Quaternion.identity;
        RoleManager.CurrentRole = newRole;
        newRole.Init(Hub,RoleChangeReason.RoundStart);
        newRole.SpawnPoolObject();

        if (data == null && newRole is FpcStandardRoleBase)
        {
            var writer = new NetworkWriter();
            
            switch (newRole)
            {
                case HumanRole { UsesUnitNames: true }:
                    writer.WriteByte(prevRole is HumanRole prevHuman ? prevHuman.UnitNameId : (byte)0);
                    break;
                case ZombieRole:
                    writer.WriteUInt16(prevRole is ZombieRole prevZombie ? prevZombie._syncMaxHealth : (ushort)600);
                    break;
            }

            writer.WriteRelativePosition(new RelativePosition(0, 0, 0, 0));
            
            if (prevRole is FpcStandardRoleBase prevFpcRole)
            {
                prevFpcRole.FpcModule.MouseLook.GetSyncValues(0, out var rotation, out _);
                writer.WriteUInt16(rotation);
            }
            else
            {
                writer.WriteUInt16(0);
            }

            data = new NetworkReader(writer.ToArray());
        }
        
        if (data != null && newRole is ISpawnDataReader reader)
        {
            reader.ReadSpawnData(data);
        }

        RoleManager._sendNextFrame = true;
    }

    public void SetPlayerRoleTypeAdvance(RoleTypeId role, Vector3 position)
    {
        var newRole = SetUpNewRole(role, out var prevRole);

        if (newRole is FpcStandardRoleBase fpc)
        {
            var writer = new NetworkWriter();
            
            switch (newRole)
            {
                case HumanRole { UsesUnitNames: true }:
                    writer.WriteByte(0);
                    break;
                case ZombieRole:
                    writer.WriteUInt16(600);
                    break;
            }

            writer.WriteRelativePosition(new RelativePosition(position));
            
            if (prevRole is FpcStandardRoleBase prevFpcRole)
            {
                prevFpcRole.FpcModule.MouseLook.GetSyncValues(0, out var rotation, out _);
                writer.WriteUInt16(rotation);
            }
            else
            {
                writer.WriteUInt16(0);
            }
            
            fpc.ReadSpawnData(new NetworkReader(writer.ToArray()));
        }
    }

    public void SetPlayerRoleTypeAdvance(RoleTypeId role, Vector3 position, float horizontalRotation)
    {
        var newRole = SetUpNewRole(role, out _);

        if (newRole is FpcStandardRoleBase fpc)
        {
            var writer = new NetworkWriter();
            
            switch (newRole)
            {
                case HumanRole { UsesUnitNames: true } humanRole:
                    writer.WriteByte(0);
                    break;
                case ZombieRole:
                    writer.WriteUInt16(600);
                    break;
            }

            writer.WriteRelativePosition(new RelativePosition(position));
            var relRot =
                (ushort)Mathf.RoundToInt(
                    Mathf.InverseLerp(0f, 360f, Quaternion.Euler(Vector3.up * horizontalRotation).eulerAngles.y) *
                    ushort.MaxValue);
            writer.WriteUInt16(relRot);
            
            fpc.ReadSpawnData(new NetworkReader(writer.ToArray()));
        }
    }

    public void SetPlayerRoleTypeAdvance(RoleTypeId role, Vector3 position, float horizontalRotation,Action<NetworkWriter> customData)
    {
        var newRole = SetUpNewRole(role, out _);
        if (newRole is not FpcStandardRoleBase fpc) return;
        
        var writer = new NetworkWriter();
        customData(writer);
        
        writer.WriteRelativePosition(new RelativePosition(position));
        var relRot =
            (ushort)Mathf.RoundToInt(
                Mathf.InverseLerp(0f, 360f, Quaternion.Euler(Vector3.up * horizontalRotation).eulerAngles.y) *
                ushort.MaxValue);
        writer.WriteUInt16(relRot);

        fpc.ReadSpawnData(new NetworkReader(writer.ToArray()));
    }

    private PlayerRoleBase SetUpNewRole(RoleTypeId role,out PlayerRoleBase prevRole)
    {
        prevRole = null;
        if (RoleManager._anySet)
        {
            prevRole = RoleManager.CurrentRole;
            prevRole.DisableRole(role);
        }
        var newRole = RoleManager.GetRoleBase(role);
        var newRoleTransform = newRole.transform;
        newRoleTransform.parent = transform;
        newRoleTransform.localPosition = Vector3.zero;
        newRoleTransform.localRotation = Quaternion.identity;
        RoleManager.CurrentRole = newRole;
        newRole.Init(Hub, RoleChangeReason.RemoteAdmin);
        newRole.SpawnPoolObject();
        RoleManager._sendNextFrame = true;

        return newRole;
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
        var prevRole = _customRole;
        if(role is null)
            return;
        
        RemoveCustomRole(DeSpawnReason.API);

        _customRole = role;
        _customRole.Player = this;
        _customRole.SpawnPlayer(prevRole, liteSpawn);
        _playerEvents.ChangeRole.Raise(new ChangeRoleEvent(this) { RoleId = role.Attribute.Id });
    }

    /// <summary>
    /// The Name of the Role the player currently has
    /// </summary>
    public string RoleName => CustomRole?.Attribute.Name ?? CurrentRole.RoleName;

    public string TeamName => _team.GetTeamName(TeamID);
}