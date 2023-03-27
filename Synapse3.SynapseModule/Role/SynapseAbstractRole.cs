using PlayerRoles;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using System.Collections.Generic;
using System.Linq;
using MEC;
using UnityEngine;

namespace Synapse3.SynapseModule.Role;

public abstract class SynapseAbstractRole : SynapseRole
{
    private readonly SynapseConfigService _config;
    private readonly PlayerEvents _player;
    private readonly RoleService _role;

    protected SynapseAbstractRole()
    {
        _config = Synapse.Get<SynapseConfigService>();
        _player = Synapse.Get<PlayerEvents>();
        _role = Synapse.Get<RoleService>();
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
            Player.FakeRoleManager.VisibleRoleInfo = new RoleInfo(config.VisibleRole, Player);
        }

        if (config.OwnRole != RoleTypeId.None)
        {
            Player.FakeRoleManager.OwnVisibleRoleInfo = new RoleInfo(config.OwnRole, Player);
        }

        var spawn = config.PossibleSpawns?[Random.Range(0, config.PossibleSpawns.Length)];
        if (spawn != null)
        {
            Player.FirstPersonMovement?.ServerOverridePosition(spawn.GetMapPosition(), spawn.GetMapRotation().eulerAngles);
        }
        Player.MaxHealth = config.MaxHealth;
        Player.Health = config.Health;
        Player.MaxArtificialHealth = config.MaxArtificialHealth;
        Player.ArtificialHealth = config.ArtificialHealth;
        Player.Scale = config.Scale;
        config.PossibleInventories?[Random.Range(0, config.PossibleInventories.Length)]?.Apply(Player);
        Player.GetStatBase<StaminaStat>().ClassChanged();

        if (!config.CustomDisplay)
        {
            OnSpawn(config);
            return;
        }

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
                Info = Attribute.Name + " (" + (config.UseCustomUnitName ? config.CustomUnitName : Player.UnitName) + ")",
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
        
        Player.CustomInfo.AutomaticUpdateOnChange = false;
        Player.CustomInfo.Add(NameEntry);
        Player.CustomInfo.Add(RoleEntry);

        if (GetConfig().Hierarchy)
        {
            Player.CustomInfo.Add(RoleAndUnitEntry);
            Player.CustomInfo.Add(PowerStatusEntries[PowerStatus.LowerRank]);
            Player.CustomInfo.Add(PowerStatusEntries[PowerStatus.SameRank]);
            Player.CustomInfo.Add(PowerStatusEntries[PowerStatus.HigherRank]);
        }
        Player.CustomInfo.AutomaticUpdateOnChange = true;
        Player.CustomInfo.UpdateInfo();

        OnSpawn(config);

        _player.UpdateDisplayName.Subscribe(UpdateDisplayName);
    }

    public sealed override void DeSpawn(DeSpawnReason reason)
    {
        Player.FakeRoleManager.VisibleRoleInfo = new RoleInfo(RoleTypeId.None, null);
        Player.FakeRoleManager.OwnVisibleRoleInfo = new RoleInfo(RoleTypeId.None, null);
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

        Player.CustomInfo.AutomaticUpdateOnChange = false;
        Player.CustomInfo.Remove(NameEntry);
        Player.CustomInfo.Remove(RoleEntry);
        Player.CustomInfo.Remove(RoleAndUnitEntry);

        if (PowerStatusEntries.ContainsKey(PowerStatus.LowerRank))
            Player.CustomInfo.Remove(PowerStatusEntries[PowerStatus.LowerRank]);

        if (PowerStatusEntries.ContainsKey(PowerStatus.SameRank))
            Player.CustomInfo.Remove(PowerStatusEntries[PowerStatus.SameRank]);

        if (PowerStatusEntries.ContainsKey(PowerStatus.HigherRank))
            Player.CustomInfo.Remove(PowerStatusEntries[PowerStatus.HigherRank]);
        
        Player.CustomInfo.AutomaticUpdateOnChange = true;
        Player.CustomInfo.UpdateInfo();
    }

    public override void TryEscape()
    {
        var config = GetConfig();
        if(config.EscapeRole == RoleService.NoneRole) return;
        if (!_role.IsIdRegistered(config.EscapeRole)) return;
        
        var items = Player.Inventory.Items.ToList();
        foreach (var item in items)
        {
            item.Destroy();
        }
        Player.RoleID = config.EscapeRole;
        foreach (var item in items)
        {
            Player.Inventory.GiveItem(item);
        }
    }

    public virtual string GetName() =>
        Player.NicknameSync.HasCustomName ? Player.NicknameSync._displayName + "*" : Player.NickName;

    public virtual void UpdateDisplayName(UpdateDisplayNameEvent ev)
    {
        if (ev.Player != Player) return;
        Timing.CallDelayed(Timing.WaitForOneFrame, () =>
        {
            NameEntry.Info = GetName();
            Player.CustomInfo.UpdateInfo();
        });
    }
}