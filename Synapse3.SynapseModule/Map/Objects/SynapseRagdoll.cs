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

public class SynapseRagdoll : NetworkSynapseObject, IFakeAbleObjectInfo<SynapseRagdoll.Info>
{
    public static readonly Dictionary<RoleType, Ragdoll> Prefabs = new ();

    private readonly PlayerService _player;
    private readonly MirrorService _mirror;

    public Vector3 OriginalRagdollScale { get; private set; }
    public Ragdoll Ragdoll { get; }
    public override GameObject GameObject => Ragdoll.gameObject;
    public override ObjectType Type => ObjectType.Ragdoll;
    public override NetworkIdentity NetworkIdentity => Ragdoll.netIdentity;
    public override void Refresh()
    {
        FakeInfoManger.UpdateAll();
        base.Refresh();
    }
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
    public uint RoleID { get; }
    public FakeInfoManger<Info> FakeInfoManger { get; private set; }
    public SynapsePlayer Owner => _player.GetPlayer(Ragdoll.Info.OwnerHub.playerId);

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

        Info defaultInfo;
        if (damage == DamageType.CustomReason && Ragdoll.Info.Handler is CustomReasonDamageHandler custom)
        {
            CustomReason = custom._deathReason;
            defaultInfo = new Info(Owner, nick, role, custom._deathReason);
        }
        else
        {
            CustomReason = string.Empty;
            defaultInfo = new Info(Owner, nick, role, damage);
        }
        FakeInfoManger = new FakeInfoManger<Info>(this, _player, defaultInfo);
        FakeInfoManger.UpdateAll();
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

    public void SendInfo(SynapsePlayer player, Info info)
    {
        DamageHandlerBase damageHandler = info.IsCustomReason ? 
            new CustomReasonDamageHandler(info.CustomDeathReason) :
            info.DamageType.GetUniversalDamageHandler();

        var ragdollInfo = new RagdollInfo(info.Owner, damageHandler, info.RoleType, 
            Position, Rotation, info.Nick, Ragdoll.Info.CreationTime);

        player.SendNetworkMessage(_mirror.GetCustomVarMessage(Ragdoll, writer =>
        {
            writer.WriteUInt64(1ul);
            writer.WriteRagdollInfo(ragdollInfo);
        }));
    }

    public struct Info
    {
        public RoleType RoleType { get; set; }
        public SynapsePlayer Owner { get; set; }
        public string Nick { get; set; }
        public DamageType DamageType { get; set; }
        public bool IsCustomReason => DamageType == DamageType.CustomReason;

        private string _customDeathReason;
        public string CustomDeathReason 
        { 
            get => IsCustomReason ? _customDeathReason : String.Empty;
            set => _customDeathReason = value; 
        }

        public Info(SynapsePlayer owner, string nick, RoleType roleType, string deathReason)
        {
            RoleType = roleType;
            Owner = owner;
            Nick = nick;
            DamageType = DamageType.CustomReason;
            _customDeathReason = deathReason;
        }

        public Info(SynapsePlayer owner, string nick, RoleType roleType, DamageType deathReason)
        {
            RoleType = roleType;
            Owner = owner;
            Nick = nick;
            DamageType = deathReason;
            _customDeathReason = String.Empty;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj)) return true;

            if (obj is not Info info) return false;

            return RoleType == info.RoleType && Owner == info.Owner && Nick == info.Nick &&
                DamageType == info.DamageType && CustomDeathReason == info.CustomDeathReason;
        }

        public override int GetHashCode()
        {
            int hashCode = -990623893;
            hashCode = hashCode * -1521134295 + RoleType.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<SynapsePlayer>.Default.GetHashCode(Owner);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Nick);
            hashCode = hashCode * -1521134295 + DamageType.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CustomDeathReason);
            return hashCode;
        }

    }

}