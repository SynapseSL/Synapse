using System.Collections.Generic;
using Mirror;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapseRagdoll : NetworkSynapseObject
{
    public static Dictionary<RoleType, Ragdoll> Prefabs = new ();

    private readonly PlayerService _player;

    public Vector3 OriginalRagdollScale { get; private set; }
    public Ragdoll Ragdoll { get; }
    public override GameObject GameObject => Ragdoll.gameObject;
    public override ObjectType Type => ObjectType.Ragdoll;
    public override NetworkIdentity NetworkIdentity => Ragdoll.netIdentity;
    public override void Refresh()
    {
        Ragdoll.NetworkInfo = new RagdollInfo(_player.Host, DamageType.GetUniversalDamageHandler(), RoleType, Position, Rotation, Nick, Ragdoll.NetworkInfo.CreationTime);
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

    public SynapsePlayer Owner => _player.GetPlayer(Ragdoll.Info.OwnerHub.playerId);

    public SynapseRagdoll(RoleType role, DamageType damage, Vector3 pos, Quaternion rot, Vector3 scale, string nick) : this()
    {
        Ragdoll = CreateRagdoll(role, damage, pos, rot, scale, nick);
        SetUp(role, damage, nick);
    }

    internal SynapseRagdoll(Ragdoll ragdoll) : this()
    {
        Ragdoll = ragdoll;
        SetUp(ragdoll.NetworkInfo.RoleType, ragdoll.NetworkInfo.Handler.GetDamageType(), ragdoll.NetworkInfo.Nickname);
    }

    private SynapseRagdoll()
    {
        _player = Synapse.Get<PlayerService>();
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
    }

    private Ragdoll CreateRagdoll(RoleType role, DamageType damage, Vector3 pos, Quaternion rot, Vector3 scale,
        string nick)
    {
        var rag = CreateNetworkObject(Prefabs[role], pos, rot, scale);
        rag.NetworkInfo = new RagdollInfo(_player.Host, damage.GetUniversalDamageHandler(), role, pos, rot, nick,
            NetworkTime.time);
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
}