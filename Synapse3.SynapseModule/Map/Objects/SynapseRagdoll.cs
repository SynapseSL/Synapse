using System.Collections.Generic;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapseRagDoll : NetworkSynapseObject
{
    public static Dictionary<RoleTypeId, BasicRagdoll> Prefabs = new();

    private readonly PlayerService _player;
    private SynapseRagDoll() => _player = Synapse.Get<PlayerService>();

    public BasicRagdoll BasicRagDoll { get; }

    public override GameObject GameObject => BasicRagDoll.gameObject;
    public override NetworkIdentity NetworkIdentity => BasicRagDoll.netIdentity;
    public override ObjectType Type => ObjectType.RagDoll;

    public SynapsePlayer Owner { get; }
    public Vector3 OriginalRagDollScale { get; private set; }

    public override Vector3 Scale
    {
        get => RevertScale(base.Scale);
        set => base.Scale = CreateScale(value);
    }

    public double CreationTime
    {
        get => BasicRagDoll.Info.CreationTime;
        set => BasicRagDoll.NetworkInfo = new RagdollData(BasicRagDoll.Info.OwnerHub, BasicRagDoll.Info.Handler,
            BasicRagDoll.Info.RoleType, BasicRagDoll.Info.StartPosition, BasicRagDoll.Info.StartRotation,
            BasicRagDoll.Info.Nickname, value);
    }
    
    public RoleTypeId Role { get; }

    public RoleTypeId DisplayedRole
    {
        get => BasicRagDoll.Info.RoleType;
        set => BasicRagDoll.NetworkInfo = new RagdollData(BasicRagDoll.Info.OwnerHub, BasicRagDoll.Info.Handler,
            value, BasicRagDoll.Info.StartPosition, BasicRagDoll.Info.StartRotation,
            BasicRagDoll.Info.Nickname, BasicRagDoll.Info.CreationTime);
    }

    public DamageType DamageType => Damage.GetDamageType();

    public string DamageText
    {
        get => (Damage as CustomReasonDamageHandler)?._deathReason ?? Damage.GetType().Name;
        set => Damage = new CustomReasonDamageHandler(value);
    }
    public DamageHandlerBase Damage
    {
        get => BasicRagDoll.Info.Handler;
        set => BasicRagDoll.NetworkInfo = new RagdollData(BasicRagDoll.Info.OwnerHub, value,
            BasicRagDoll.Info.RoleType, BasicRagDoll.Info.StartPosition, BasicRagDoll.Info.StartRotation,
            BasicRagDoll.Info.Nickname, BasicRagDoll.Info.CreationTime);
    }

    public string NickName
    {
        get => BasicRagDoll.Info.Nickname;
        set => BasicRagDoll.NetworkInfo = new RagdollData(BasicRagDoll.Info.OwnerHub, BasicRagDoll.Info.Handler,
            BasicRagDoll.Info.RoleType, BasicRagDoll.Info.StartPosition, BasicRagDoll.Info.StartRotation,
            value, BasicRagDoll.Info.CreationTime);
    }

    public float ExistenceTime => BasicRagDoll.Info.ExistenceTime;

    public void SendFakeInfoToPlayer(SynapsePlayer player, RagdollData data) =>
        player.SendFakeSyncVar(BasicRagDoll, 1u, data);

    internal SynapseRagDoll(BasicRagdoll ragDoll) : this()
    {
        BasicRagDoll = ragDoll;
        Owner = ragDoll.Info.OwnerHub?.GetSynapsePlayer();
        Map._synapseRagdolls.Add(this);
        SetUp();
    }

    public SynapseRagDoll(RoleTypeId role, Vector3 position, Quaternion rotation, Vector3 scale, SynapsePlayer owner,
        DamageHandlerBase damage, string nick, bool disableFadeOut = true) : this()
    {
        var infoOwner = !disableFadeOut && owner != null ? owner : _player.Host;
        BasicRagDoll = CreateRagDoll(role, position, rotation, scale,
            infoOwner.Hub, damage, nick);
        Owner = owner;
        SetUp();
    }

    public SynapseRagDoll(RoleTypeId role, Vector3 position, Quaternion rotation, Vector3 scale, SynapsePlayer owner,
        DamageType damage, string nick, bool disableFadeOut = true) : this(role, position, rotation, scale, owner,
        damage.GetUniversalDamageHandler(), nick, disableFadeOut) { }
    
    public SynapseRagDoll(RoleTypeId role, Vector3 position, Quaternion rotation, Vector3 scale, SynapsePlayer owner,
        string damageText, string nick, bool disableFadeOut = true) : this(role, position, rotation, scale, owner,
        new CustomReasonDamageHandler(damageText), nick, disableFadeOut) { }

    internal SynapseRagDoll(SchematicConfiguration.RagdollConfiguration configuration,
        SynapseSchematic schematic) :
        this(configuration.RoleType, configuration.Position, configuration.Rotation,
            configuration.Scale, null, configuration.DamageType, configuration.Nick)
    {
        Parent = schematic;
        schematic._ragdolls.Add(this);

        Scale = configuration.Scale;
        CustomAttributes = configuration.CustomAttributes;
    }

    private BasicRagdoll CreateRagDoll(RoleTypeId role, Vector3 pos, Quaternion rotation, Vector3 scale,
        ReferenceHub owner, DamageHandlerBase damageHandlerBase, string nick)
    {
        var rag = CreateNetworkObject(Prefabs[role], pos, rotation, scale);
        rag.Info = new RagdollData(owner, damageHandlerBase, role, pos, rotation, nick, NetworkTime.time);
        Map._synapseRagdolls.Add(this);
        NetworkServer.Spawn(rag.gameObject);
        return rag;
    }
    
    protected override TComponent CreateNetworkObject<TComponent>(TComponent component, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        var gameObject = Object.Instantiate(component, pos, rot);
        OriginalRagDollScale = gameObject.transform.localScale;
        gameObject.transform.localScale = CreateScale(scale);
        return gameObject;
    }

    public override void Refresh()
    {
        base.Refresh();
        BasicRagDoll.NetworkInfo = BasicRagDoll.NetworkInfo;
    }
    
    public override void OnDestroy()
    {
        Map._synapseRagdolls.Remove(this);
        base.OnDestroy();
        
        if (Parent is SynapseSchematic schematic) schematic._ragdolls.Remove(this);
    }

    private Vector3 CreateScale(Vector3 newScale)
    {
        newScale.x *= OriginalRagDollScale.x;
        newScale.y *= OriginalRagDollScale.y;
        newScale.z *= OriginalRagDollScale.z;
        return newScale;
    }
    
    private Vector3 RevertScale(Vector3 currentScale)
    {
        currentScale.x /= OriginalRagDollScale.x;
        currentScale.y /= OriginalRagDollScale.y;
        currentScale.z /= OriginalRagDollScale.z;
        return currentScale;
    }
    
    private void SetUp()
    {
        GameObject.AddComponent<SynapseObjectScript>().Object = this;
        MoveInElevator = true;
    }
}