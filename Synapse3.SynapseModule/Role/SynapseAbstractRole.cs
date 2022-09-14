using System.Collections.Generic;
using System.Linq;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Teams.Unit;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Synapse3.SynapseModule.Role;

public abstract class SynapseAbstractRole : SynapseRole
{
    protected readonly UnitService _unit;

    protected SynapseAbstractRole()
    {
        _unit = Synapse.Get<UnitService>();
    }
    
    public CustomInfoList.CustomInfoEntry NameEntry { get; private set; }
    public CustomInfoList.CustomInfoEntry RoleEntry { get; private set; }
    public CustomInfoList.CustomInfoEntry RoleAndUnitEntry { get; private set; }

    private IAbstractRoleConfig GetConfig()
    {
        var config = GetRoleConfig() ?? Attribute as AbstractRoleAttribute;
        return config;
    }

    protected virtual IAbstractRoleConfig GetRoleConfig() => null;
    
    protected virtual void PreSpawn() { }
    
    protected virtual void OnSpawn() { }

    protected virtual bool CanSeeUnit(SynapsePlayer player) => true;
    protected virtual bool CanNotSeeUnit(SynapsePlayer player) => !CanSeeUnit(player);
    
    protected virtual void AddPowerStatusInfo() { }

    public sealed override void SpawnPlayer(bool spawnLite)
    {
        if (spawnLite) return;
        var config = GetConfig();
        if (config == null)
        {
            Player.RoleID = (uint)RoleType.Spectator;
            return;
        }

        PreSpawn();
        var hasUnit = config.UnitId != 0 && !string.IsNullOrWhiteSpace(config.Unit);
        if (hasUnit)
        {
            if (!_unit.UnitList.Any(x => x.SpawnableTeam == config.UnitId && x.UnitName == config.Unit))
            {
                _unit.AddUnit(config.Unit, config.UnitId, int.MaxValue);
            }

            Player.UnitId = config.UnitId;
            Player.Unit = config.Unit;
        }
        Player.ChangeRoleLite(config.Role);
        var spawn = config.PossibleSpawns?[Random.Range(0, config.PossibleSpawns.Length)];
        if (spawn != null)
        {
            Player.Position = spawn.GetMapPosition();
            Player.Rotation = spawn.GetMapRotation();   
        }
        Player.Health = config.Health;
        Player.MaxHealth = config.MaxHealth;
        Player.ArtificialHealth = config.ArtificialHealth;
        Player.MaxArtificialHealth = config.MaxArtificialHealth;
        config.PossibleInventories?[Random.Range(0, config.PossibleInventories.Length)]?.Apply(Player);
        Player.GetComponent<FirstPersonController>().ResetStamina();
        
        Player.RemoveDisplayInfo(PlayerInfoArea.Nickname);
        Player.RemoveDisplayInfo(PlayerInfoArea.Role);
        Player.RemoveDisplayInfo(PlayerInfoArea.UnitName);

        NameEntry = new CustomInfoList.CustomInfoEntry()
        {
            Info = Player.NickName
        };
        if (hasUnit)
        {
            RoleEntry = new CustomInfoList.CustomInfoEntry()
            {
                EveryoneCanSee = false,
                Info = Attribute.Name,
                SeeCondition = CanNotSeeUnit
            };
            RoleAndUnitEntry = new CustomInfoList.CustomInfoEntry()
            {
                EveryoneCanSee = false,
                Info = Attribute.Name + " (" + config.Unit + ")",
                SeeCondition = CanSeeUnit
            };
        }
        else
        {
            RoleEntry = new CustomInfoList.CustomInfoEntry()
            {
                Info = Attribute.Name
            };
        }

        Player.CustomInfo.Add(NameEntry);
        Player.CustomInfo.Add(RoleEntry);
        if (hasUnit)
        {
            Player.CustomInfo.Add(RoleAndUnitEntry);
            AddPowerStatusInfo();
        }
        OnSpawn();
    }

    public sealed override void DeSpawn(DespawnReason reason)
    {
        Player.AddDisplayInfo(PlayerInfoArea.Nickname);
        Player.AddDisplayInfo(PlayerInfoArea.Role);
        Player.AddDisplayInfo(PlayerInfoArea.UnitName);
    }

    public override void TryEscape()
    {
        Player.RoleID = GetConfig().EscapeRole;
    }
}

[AbstractRole(
    Health = 75,
    Id = 99,
    Name = "TestRole",
    Role = RoleType.Scientist,
    Unit = "Scientist Unit\nTest",
    UnitId = 5,
    ArtificialHealth = 100,
    MaxArtificialHealth = 200,
    EscapeRole = (uint)RoleType.NtfCaptain,
    MaxHealth = 200,
    TeamId = (uint)Team.RSC
)]
public class TestRole : SynapseAbstractRole
{
    public override void Load()
    {
        (Attribute as AbstractRoleAttribute).PossibleInventories = new[]
        {
            new SerializedPlayerInventory()
            {
                Ammo = new SerializedAmmo()
                {
                    Ammo5 = 10,
                },
                Items = new List<SerializedPlayerItem>()
                {
                    new SerializedPlayerItem()
                    {
                        ID = (uint)ItemType.Coin,
                        Chance = 100,
                    }
                }
            }
        };

        (Attribute as AbstractRoleAttribute).PossibleSpawns = new[]
        {
            new RoomPoint()
            {
                roomName = "Shelter",
                position = Vector3.up * 2,
                rotation = Quaternion.Euler(Vector3.left)
            }
        };
    }
}