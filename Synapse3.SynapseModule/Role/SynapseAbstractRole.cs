using PlayerRoles;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Synapse3.SynapseModule.Role;

public abstract class SynapseAbstractRole : SynapseRole
{
    private readonly SynapseConfigService _config;
    private readonly PlayerEvents _player;

    protected SynapseAbstractRole()
    {
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

    protected virtual void OnDeSpawn(DeSpawnReason reason) { }

    protected virtual bool CanSeeUnit(SynapsePlayer player) => player.TeamID == Attribute.TeamId;
    protected virtual bool CanNotSeeUnit(SynapsePlayer player) => !CanSeeUnit(player);

    protected virtual bool LowerRank(SynapsePlayer player) => CanSeeUnit(player) && player.RoleID < Player.RoleID;
    protected virtual bool SameRank(SynapsePlayer player) => CanSeeUnit(player) && player.RoleID == Player.RoleID;
    protected virtual bool HigherRank(SynapsePlayer player) => CanSeeUnit(player) && player.RoleID > Player.RoleID;

    public sealed override void SpawnPlayer(ISynapseRole previousRole = null, bool spawnLite = false)
    {
        if (spawnLite) return;
        var config = GetConfig();
        if (config == null)
        {
            Player.RoleID = (uint)RoleTypeId.Spectator;
            return;
        }

        PreSpawn();
        Player.SetRoleFlags(config.Role, RoleSpawnFlags.None);
        if (config.VisibleRole != RoleTypeId.None)
        {
            Player.FakeRoleManager.VisibleRole = new RoleInfo(config.VisibleRole, Player);
        }

        if (config.OwnRole != RoleTypeId.None)
        {
            Player.FakeRoleManager.OwnVisibleRole = new RoleInfo(config.OwnRole, Player);
        }

        var spawn = config.PossibleSpawns?[Random.Range(0, config.PossibleSpawns.Length)];
        if (spawn != null)
        {
            Player.FirstPersonMovement?.ServerOverridePosition(spawn.GetMapPosition(), spawn.GetMapRotation().eulerAngles);
        }
        Player.Health = config.Health;
        Player.MaxHealth = config.MaxHealth;
        Player.ArtificialHealth = config.ArtificialHealth;
        Player.MaxArtificialHealth = config.MaxArtificialHealth;
        Player.Scale = config.Scale;
        config.PossibleInventories?[Random.Range(0, config.PossibleInventories.Length)]?.Apply(Player);
        Player.GetStatBase<StaminaStat>().ClassChanged();

        Player.RemoveDisplayInfo(PlayerInfoArea.Nickname);
        Player.RemoveDisplayInfo(PlayerInfoArea.Role);
        Player.RemoveDisplayInfo(PlayerInfoArea.UnitName);
        Player.RemoveDisplayInfo(PlayerInfoArea.PowerStatus);

        NameEntry = new CustomInfoList.CustomInfoEntry()
        {
            Info = GetName()
        };
        if (GetConfig().Hierarchy)
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
                Info = Attribute.Name + " (" + Player.UnitName + ")",
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

        if (GetConfig().Hierarchy)
        {
            Player.CustomInfo.Add(RoleAndUnitEntry);
            Player.CustomInfo.Add(PowerStatusEntries[PowerStatus.LowerRank]);
            Player.CustomInfo.Add(PowerStatusEntries[PowerStatus.SameRank]);
            Player.CustomInfo.Add(PowerStatusEntries[PowerStatus.HigherRank]);
        }

        OnSpawn(config);

        _player.UpdateDisplayName.Subscribe(UpdateDisplayName);
    }

    public sealed override void DeSpawn(DeSpawnReason reason)
    {
        Player.FakeRoleManager.VisibleRole = new RoleInfo(RoleTypeId.None, null);
        Player.FakeRoleManager.OwnVisibleRole = new RoleInfo(RoleTypeId.None, null);
        Player.Scale = Vector3.one;

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

        if (PowerStatusEntries.ContainsKey(PowerStatus.LowerRank))
            Player.CustomInfo.Remove(PowerStatusEntries[PowerStatus.LowerRank]);

        if (PowerStatusEntries.ContainsKey(PowerStatus.SameRank))
            Player.CustomInfo.Remove(PowerStatusEntries[PowerStatus.SameRank]);

        if (PowerStatusEntries.ContainsKey(PowerStatus.HigherRank))
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
    }

    public virtual string GetName() =>
        Player.NicknameSync.HasCustomName ? Player.NicknameSync._displayName : Player.NickName;

    public virtual void UpdateDisplayName(UpdateDisplayNameEvent ev)
    {
        NameEntry.Info = GetName();
        Player.CustomInfo.UpdateInfo();
    }
}