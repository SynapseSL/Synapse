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
    private readonly Dictionary<SynapsePlayer, DamageHandlerBase> _sendInfo = new();

    public Vector3 OriginalRagdollScale { get; private set; }
    public Ragdoll Ragdoll { get; }
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
    
    public DamageType DamageType { get; private set; }
    public RoleType RoleType { get; private set; }
    public string Nick { get; private set; }
    public string CustomReason { get; private set; }
    public bool CanBeRevive { get; set; }
    public bool CanBeReviveInTime => Ragdoll.Info.ExistenceTime <= Scp049.ReviveEligibilityDuration;
    public uint RoleID { get; private set; }
    public SynapsePlayer Owner => _player.GetPlayer(Ragdoll.Info.OwnerHub.playerId);
    public Dictionary<Func<SynapsePlayer, bool>, Info> VisibleInfoCondition { get; set; } = new();

    public bool NeedsJoinUpdate => VisibleInfoCondition.Count != 0;

    public SynapseRagdoll(RoleType role, string reason, Vector3 pos, Quaternion rot, Vector3 scale,
        string nick, SynapsePlayer player = null, bool canBeRevive = false, uint roleID = RoleService.NoneRole) : this()
    {
        CanBeRevive = canBeRevive;
        RoleID = roleID;
        Ragdoll = CreateRagdoll(role, reason, pos, rot, scale, nick, player);
        SetUp(role, DamageType.CustomReason, nick);
    }

    public SynapseRagdoll(RoleType role, DamageType damage, Vector3 pos, Quaternion rot, Vector3 scale, 
        string nick, SynapsePlayer player = null, bool canBeRevive = false, uint roleID = RoleService.NoneRole) : this()
    {
        CanBeRevive = canBeRevive;
        RoleID = roleID;
        Ragdoll = CreateRagdoll(role, damage, pos, rot, scale, nick, player);
        SetUp(role, damage, nick);
    }

    internal SynapseRagdoll(Ragdoll ragdoll, uint roleID, bool canBeRive) : this()
    {
        RoleID = roleID;
        CanBeRevive = canBeRive;
        Ragdoll = ragdoll;
        SetUp(ragdoll.Info.RoleType, ragdoll.Info.Handler.GetDamageType(), ragdoll.Info.Nickname);
    }

    internal SynapseRagdoll(Ragdoll ragdoll, bool isAfterDeath = false) : this()
    {
        Ragdoll = ragdoll;
        if (isAfterDeath && Owner != null)
        {
            CanBeRevive = Owner.TeamID != (int)Team.SCP;
            RoleID = Owner.RoleID;
        }
        else
        {
            CanBeRevive = false;
            RoleID = RoleService.NoneRole;
        }

        SetUp(ragdoll.Info.RoleType, ragdoll.Info.Handler.GetDamageType(), ragdoll.Info.Nickname);
    }


    private SynapseRagdoll()
    {
        _player = Synapse.Get<PlayerService>();
        _mirror = Synapse.Get<MirrorService>();
        _player.JoinUpdates.Add(this);
    }

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

    private void SetUp(RoleType role, DamageType damage, string nick)
    {
        Map._synapseRagdolls.Add(this);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;

        DamageType = damage;
        RoleType = role;
        Nick = nick;
        MoveInElevator = true;
        CanBeRevive = true;
        //Position = Ragdoll.Info.StartPosition;

        if (damage == DamageType.CustomReason && Ragdoll.Info.Handler is CustomReasonDamageHandler custom)
            CustomReason = custom._deathReason;
        else
            CustomReason = string.Empty;
        
    }

    private Ragdoll CreateRagdoll(RoleType role, DamageType damage, Vector3 pos, Quaternion rot, Vector3 scale,
        string nick, SynapsePlayer player)
    {
        var rag = CreateNetworkObject(Prefabs[role], pos, rot, scale);
        rag.Info = new RagdollInfo(player ?? _player.Host, damage.GetUniversalDamageHandler(), role, pos, rot, nick,
            NetworkTime.time);
        return rag;
    }

    private Ragdoll CreateRagdoll(RoleType role, string deathReason, Vector3 pos, Quaternion rot, Vector3 scale,
        string nick, SynapsePlayer player)
    {
        var rag = CreateNetworkObject(Prefabs[role], pos, rot, scale);
        rag.Info = new RagdollInfo(player ?? _player.Host, new PlayerStatsSystem.CustomReasonDamageHandler(deathReason), 
            role, pos, rot, nick, NetworkTime.time);
        return rag;
    }

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

    public void UpdateInfo()
    {
        foreach (var player in _player.Players)
            UpdateInfoPlayer(player);
    }

    public void UpdatePlayer(SynapsePlayer player) => UpdateInfoPlayer(player);

    private void UpdateInfoPlayer(SynapsePlayer player)
    {
        var vanilaInfo = Ragdoll.Info;
        var damgHandler = vanilaInfo.Handler;

        foreach (var condition in VisibleInfoCondition)
        {
            if (condition.Key.Invoke(player))
            {
                var info = condition.Value;
                damgHandler = condition.Value.DamageType == DamageType.CustomReason ?
                    new CustomReasonDamageHandler(info.CustomInfo) :
                    info.DamageType.GetUniversalDamageHandler();
                break;
            }
        }

        //This will prevent to send unnecessary packages from being send
        if (!_sendInfo.ContainsKey(player)) _sendInfo.Add(player, default);
        else if (_sendInfo[player] == damgHandler) return;
        _sendInfo[player] = damgHandler;

        RagdollInfo ragdollInfo;

        // When you resend a RagdollInfo to the player owner of the ragdoll, the client thinks it just died and fades to black
        /*            if (vanilaInfo.OwnerHub == player) 
                    {
                        ragdollInfo = new RagdollInfo(_player.Host, damgHandler, vanilaInfo.RoleType,
                            vanilaInfo.StartPosition, vanilaInfo.StartRotation, vanilaInfo.Nickname, Ragdoll.Info.CreationTime);
                    }
                    else*/
        {
            ragdollInfo = new RagdollInfo(vanilaInfo.OwnerHub, damgHandler, vanilaInfo.RoleType,
               vanilaInfo.StartPosition, vanilaInfo.StartRotation, vanilaInfo.Nickname, Ragdoll.Info.CreationTime);
        }

        player.SendNetworkMessage(_mirror.GetCustomVarMessage(Ragdoll, writer =>
        {
            writer.WriteUInt64(1ul);
            writer.WriteRagdollInfo(ragdollInfo);
        }));
    }

    public struct Info
    {
        public Info(string reason)
        {
            CustomInfo = reason;
            DamageType = DamageType.CustomReason;
        }

        public Info(DamageType reason)
        {
            CustomInfo = string.Empty;
            DamageType = reason;
        }

        public string CustomInfo { get; set; }

        public DamageType DamageType { get; set; }
    }

}