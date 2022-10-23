using System;
using System.Collections.Generic;
using Mirror;
using Neuron.Core.Logging;
using PlayableScps;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapseRagdoll : NetworkSynapseObject, IJoinUpdate
{
    public static Dictionary<RoleType, Ragdoll> Prefabs = new ();

    private readonly PlayerService _player;
    private readonly MirrorService _mirror;
    private readonly Dictionary<SynapsePlayer, SynapseRagDollInfo> _sendInfo = new();

    #region SynapseObject
    public override GameObject GameObject => Ragdoll.gameObject;
    
    public override ObjectType Type => ObjectType.Ragdoll;
    
    public override NetworkIdentity NetworkIdentity => Ragdoll.netIdentity;
    
    public override void OnDestroy()
    {
        Map._synapseRagdolls.Remove(this);
        base.OnDestroy();
        
        if (Parent is SynapseSchematic schematic) schematic._ragdolls.Remove(this);
    }
    
    public override Vector3 Scale
    {
        get => RevertScale(base.Scale);
        set => base.Scale = CreateScale(value);
    }
    #endregion

    #region RagDollProperties
    public Ragdoll Ragdoll { get; }
    public Vector3 OriginalRagdollScale { get; private set; }
    public DamageType DamageType { get; private set; }
    public RoleType RoleType { get; private set; }
    public string Nick { get; private set; }
    public string CustomReason => (Info?.DamageHandler as CustomReasonDamageHandler)?._deathReason ?? string.Empty;
    public bool CanBeRevive { get; set; }
    public bool CanBeReviveInTime => Ragdoll.Info.ExistenceTime <= Scp049.ReviveEligibilityDuration;
    public uint RoleID { get; private set; }
    public SynapsePlayer Owner { get; private set; }
    #endregion

    #region SyncVar
    private SynapseRagDollInfo _synapseRagDollInfo;

    public SynapseRagDollInfo Info
    {
        get => _synapseRagDollInfo;
        set
        {
            Ragdoll.Info = new RagdollInfo(value.UseHostId ? _player.Host : Owner, value.DamageHandler,
                value.DisplayedRole, Ragdoll.Info.StartPosition, Ragdoll.Info.StartRotation, value.NickName,
                Ragdoll.Info.CreationTime);
            _synapseRagDollInfo = value;
            UpdateInfo();
        }
    }
    public Dictionary<Func<SynapsePlayer, bool>, SynapseRagDollInfo> VisibleInfoCondition { get; set; } = new();

    public bool NeedsJoinUpdate => VisibleInfoCondition.Count != 0;
    
    public void UpdateInfo()
    {
        foreach (var player in _player.Players)
            UpdatePlayer(player);
    }

    public void UpdatePlayer(SynapsePlayer player)
    {
        var infoToSend = Info;

        if (infoToSend == null)
        {
            NeuronLogger.For<SynapseRagdoll>().Debug("Info of RagDoll is null can't spawn RagDoll on Players Client");
            return;
        }

        foreach (var condition in VisibleInfoCondition)
        {
            if (!condition.Key.Invoke(player)) continue;
            infoToSend = condition.Value;
            break;
        }

        //This will prevent to send unnecessary packages from being send
        if (!_sendInfo.ContainsKey(player)) _sendInfo.Add(player, default);
        else if (_sendInfo[player] == infoToSend) return;
        _sendInfo[player] = infoToSend;

        var ragdollInfo = new RagdollInfo(infoToSend.UseHostId ? _player.Host : Ragdoll.Info.OwnerHub, infoToSend.DamageHandler, infoToSend.DisplayedRole,
            Ragdoll.Info.StartPosition, Ragdoll.Info.StartRotation, infoToSend.NickName, Ragdoll.Info.CreationTime);

        player.SendNetworkMessage(_mirror.GetCustomVarMessage(Ragdoll, writer =>
        {
            writer.WriteUInt64(1ul);
            writer.WriteRagdollInfo(ragdollInfo);
        }));
    }

    public class SynapseRagDollInfo
    {
        public SynapseRagDollInfo(string reason, string nick, RoleType displayedRole)
        {
            DamageHandler = new CustomReasonDamageHandler(reason);
            NickName = nick;
            DisplayedRole = displayedRole;
        }

        public SynapseRagDollInfo(DamageHandlerBase damageHandler, string nick, RoleType displayedRole)
        {
            DamageHandler = damageHandler;
            NickName = nick;
            DisplayedRole = displayedRole;
        }

        public DamageHandlerBase DamageHandler { get; set; }
        
        public string NickName { get; set; }
        
        public RoleType DisplayedRole { get; set; }
        
        public bool UseHostId { get; set; }

        public SynapseRagDollInfo Copy() => new SynapseRagDollInfo(DamageHandler, NickName, DisplayedRole);

        public static SynapseRagDollInfo Of(RagdollInfo info) =>
            new SynapseRagDollInfo(info.Handler, info.Nickname, info.RoleType);
    }
    #endregion

    #region Constructor
    //Always called
    private SynapseRagdoll()
    {
        _player = Synapse.Get<PlayerService>();
        _mirror = Synapse.Get<MirrorService>();
        _player.JoinUpdates.Add(this);
    }
    
    //Public for Plugins
    public SynapseRagdoll(RoleType role, string reason, Vector3 pos, Quaternion rot, Vector3 scale,
        string nick, SynapsePlayer player = null, bool canBeRevive = false, uint roleID = RoleService.NoneRole, bool enableFadeOut = true) : this()
    {
        Ragdoll = CreateRagdoll(role, pos, rot, scale);
        SetUp(role, DamageType.CustomReason, nick, player ?? _player.Host, !enableFadeOut, canBeRevive, roleID,
            new CustomReasonDamageHandler(reason));
    }

    public SynapseRagdoll(RoleType role, DamageType damage, Vector3 pos, Quaternion rot, Vector3 scale, 
        string nick, SynapsePlayer player = null, bool canBeRevive = false, uint roleID = RoleService.NoneRole, bool enableFadeOut = true) : this()
    {
        Ragdoll = CreateRagdoll(role, pos, rot, scale);
        SetUp(role, damage, nick, player ?? _player.Host, !enableFadeOut, canBeRevive, roleID);
    }
    
    //Only vanila Ragdoll
    internal SynapseRagdoll(Ragdoll ragdoll) : this()
    {
        Ragdoll = ragdoll;
        var owner = _player.GetPlayer(Ragdoll.Info.OwnerHub.playerId);
        SetUp(ragdoll.Info.RoleType, ragdoll.Info.Handler.GetDamageType(), ragdoll.Info.Nickname, owner, false,
            owner.TeamID != (uint)Team.SCP, owner.RoleID, ragdoll.Info.Handler);
    }

    //Schematic
    internal SynapseRagdoll(SchematicConfiguration.RagdollConfiguration configuration,
        SynapseSchematic schematic) :
        this(configuration.RoleType, configuration.DamageType, configuration.Position, configuration.Rotation,
            configuration.Scale, configuration.Nick)
    {
        Parent = schematic;
        schematic._ragdolls.Add(this);

        OriginalScale = configuration.Scale;
        CustomAttributes = configuration.CustomAttributes;
    }

    private void SetUp(RoleType role, DamageType damage, string nick, SynapsePlayer owner, bool useHost,
        bool canRevive, uint customRole, DamageHandlerBase handlerBase = null)
    {
        Map._synapseRagdolls.Add(this);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;

        DamageType = damage;
        RoleType = role;
        Nick = nick;
        Owner = owner;
        CanBeRevive = canRevive;
        RoleID = customRole;
        MoveInElevator = true;
        
        Ragdoll.Info = new RagdollInfo(owner, handlerBase ?? damage.GetUniversalDamageHandler(), role, Position, Rotation,
            nick, NetworkTime.time);
        var info = SynapseRagDollInfo.Of(Ragdoll.Info);
        info.UseHostId = useHost;
        _synapseRagDollInfo = info;
        UpdateInfo();
    }

    private Ragdoll CreateRagdoll(RoleType role, Vector3 pos, Quaternion rot, Vector3 scale) =>
        CreateNetworkObject(Prefabs[role], pos, rot, scale);

    protected override TComponent CreateNetworkObject<TComponent>(TComponent component, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        OriginalRagdollScale = component.transform.localScale;
        return base.CreateNetworkObject(component, pos, rot, CreateScale(scale));
    }

    private Vector3 CreateScale(Vector3 newScale)
    {
        newScale.x *= OriginalRagdollScale.x;
        newScale.y *= OriginalRagdollScale.y;
        newScale.z *= OriginalRagdollScale.z;
        return newScale;
    }

    private Vector3 RevertScale(Vector3 currentScale)
    {
        currentScale.x *= OriginalRagdollScale.x;
        currentScale.y *= OriginalRagdollScale.y;
        currentScale.z *= OriginalRagdollScale.z;
        return currentScale;
    }
    #endregion
}