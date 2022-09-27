﻿using System.Collections.Generic;
using System.Linq;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Teams.Unit;
using Random = UnityEngine.Random;

namespace Synapse3.SynapseModule.Role;

public abstract class SynapseAbstractRole : SynapseRole
{
    private readonly UnitService _unit;
    private readonly SynapseConfigService _config;
    private readonly PlayerEvents _player;

    protected SynapseAbstractRole()
    {
        _unit = Synapse.Get<UnitService>();
        _config = Synapse.Get<SynapseConfigService>();
        _player = Synapse.Get<PlayerEvents>();
    }
    
    public CustomInfoList.CustomInfoEntry NameEntry { get; private set; }
    public CustomInfoList.CustomInfoEntry RoleEntry { get; private set; }
    public CustomInfoList.CustomInfoEntry RoleAndUnitEntry { get; private set; }

    public Dictionary<PowerStatus, CustomInfoList.CustomInfoEntry> PowerStatusEntries { get; } = new()
    {
        { PowerStatus.Invisible, null }
    };

    protected abstract IAbstractRoleConfig GetConfig();
    
    protected virtual void PreSpawn() { }
    
    protected virtual void OnSpawn(IAbstractRoleConfig config) { }
    
    protected virtual void OnDeSpawn(DespawnReason reason) { }

    protected virtual bool CanSeeUnit(SynapsePlayer player) => player.UnitId == GetConfig().UnitId;
    protected virtual bool CanNotSeeUnit(SynapsePlayer player) => !CanSeeUnit(player);

    protected virtual bool LowerRank(SynapsePlayer player) => CanSeeUnit(player) && player.RoleID < Player.RoleID;
    protected virtual bool SameRank(SynapsePlayer player) => CanSeeUnit(player) && player.RoleID == Player.RoleID;
    protected virtual bool HigherRank(SynapsePlayer player) => CanSeeUnit(player) && player.RoleID > Player.RoleID;

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
                _unit.AddUnit(config.Unit, config.UnitId);
            }

            Player.UnitId = config.UnitId;
            Player.Unit = config.Unit;
        }
        Player.ChangeRoleLite(config.Role);
        if (config.VisibleRole != RoleType.None)
        {
            Player.FakeRoleManager.VisibleRole = config.VisibleRole;
        }
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
        Player.RemoveDisplayInfo(PlayerInfoArea.PowerStatus);

        NameEntry = new CustomInfoList.CustomInfoEntry()
        {
            Info = GetName()
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

            PowerStatusEntries[PowerStatus.LowerRank] = new CustomInfoList.CustomInfoEntry()
            {
                EveryoneCanSee = false,
                Info = "\n" + _config.Translation.Get().LowerRank,
                SeeCondition = LowerRank
            };
            PowerStatusEntries[PowerStatus.SameRank] = new CustomInfoList.CustomInfoEntry()
            {
                EveryoneCanSee = false,
                Info = "\n" + _config.Translation.Get().SameRank,
                SeeCondition = SameRank
            };
            PowerStatusEntries[PowerStatus.HigherRank] = new CustomInfoList.CustomInfoEntry()
            {
                EveryoneCanSee = false,
                Info = "\n" + _config.Translation.Get().HigherRank,
                SeeCondition = HigherRank
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
            Player.CustomInfo.Add(PowerStatusEntries[PowerStatus.LowerRank]);
            Player.CustomInfo.Add(PowerStatusEntries[PowerStatus.SameRank]);
            Player.CustomInfo.Add(PowerStatusEntries[PowerStatus.HigherRank]);
        }

        OnSpawn(config);
        
        _player.UpdateDisplayName.Subscribe(UpdateDisplayName);
    }

    public sealed override void DeSpawn(DespawnReason reason)
    {
        Player.FakeRoleManager.VisibleRole = RoleType.None;
        
        RemoveCustomDisplay();

        OnDeSpawn(reason);
        
        _player.UpdateDisplayName.Unsubscribe(UpdateDisplayName);
    }

    public void RemoveCustomDisplay()
    {
        Player.AddDisplayInfo(PlayerInfoArea.Nickname);
        Player.AddDisplayInfo(PlayerInfoArea.Role);
        Player.AddDisplayInfo(PlayerInfoArea.UnitName);
        Player.AddDisplayInfo(PlayerInfoArea.PowerStatus);

        Player.CustomInfo.Remove(NameEntry);
        Player.CustomInfo.Remove(RoleEntry);
        Player.CustomInfo.Remove(RoleAndUnitEntry);
        Player.CustomInfo.Remove(PowerStatusEntries[PowerStatus.LowerRank]);
        Player.CustomInfo.Remove(PowerStatusEntries[PowerStatus.SameRank]);
        Player.CustomInfo.Remove(PowerStatusEntries[PowerStatus.HigherRank]);
    }

    public override void TryEscape()
    {
        var items = Player.Inventory.Items.ToList();
        foreach (var item in items)
        {
            item.Destroy();
        }
        Player.RoleID = GetConfig().EscapeRole;
        foreach (var item in items)
        {
            Player.Inventory.GiveItem(item);
        }
        Player.Escape.TargetShowEscapeMessage(Player.Connection, false, false);
    }

    public virtual string GetName() =>
        Player.NicknameSync.HasCustomName ? Player.NicknameSync._displayName : Player.NickName;

    public virtual void UpdateDisplayName(UpdateDisplayNameEvent ev)
    {
        NameEntry.Info = GetName();
        Player.CustomInfo.UpdateInfo();
    }
}

[Role(
    Name = "TestRole",
    Id = 99,
    TeamId = 10
)]
public class Test : SynapseAbstractRole
{
    protected override IAbstractRoleConfig GetConfig() => new Config()
    {
        Role = RoleType.Scientist,
        Health = 101,
        MaxHealth = 102,
        Unit = "Custom Role Wave",
        UnitId = 2,
        ArtificialHealth = 9999,
        EscapeRole = (uint)RoleType.NtfCaptain,
        PossibleInventories = new[]
        {
            new SerializedPlayerInventory()
            {
                Ammo = new SerializedAmmo()
                {
                    Ammo9 = 20,
                },
                Items = new List<SerializedPlayerItem>()
                {
                    new SerializedPlayerItem()
                    {
                        Chance = 100,
                        ProvideFully = true,
                        ID = (uint)ItemType.GunRevolver
                    }
                }
            }
        },
        MaxArtificialHealth = 9999,
        PossibleSpawns = new[]
        {
            new RoomPoint()
            {
                roomName = "Shelter",
                position = new SerializedVector3(0f, 2f, 0f),
                rotation = new SerializedVector3(0f, 180f, 0f),
            }
        }
    };

    private class Config : IAbstractRoleConfig
    {
        public RoleType Role { get; set; }
        public RoleType VisibleRole { get; set; }
        public uint EscapeRole { get; set; }
        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public float ArtificialHealth { get; set; }
        public float MaxArtificialHealth { get; set; }
        public RoomPoint[] PossibleSpawns { get; set; }
        public SerializedPlayerInventory[] PossibleInventories { get; set; }
        public byte UnitId { get; set; }
        public string Unit { get; set; }
    }
}