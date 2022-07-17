using System.Collections.Generic;
using AdminToys;
using Mirror;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapseTarget : SynapseToyObject<ShootingTarget>
{
    public static Dictionary<TargetType, ShootingTarget> Prefabs { get; } = new();
    
    public override ObjectType Type => ObjectType.Target;
    public override ShootingTarget ToyBase { get; }
    public override void OnDestroy()
    {
        Map._synapseTargets.Remove(this);
        base.OnDestroy();
        
        if (Parent is SynapseSchematic schematic) schematic._targets.Remove(this);
    }
    public TargetType SynapseTargetType { get; private set; }

    public SynapseTarget(TargetType type, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        ToyBase = CreateTarget(type, position, rotation, scale);
        SetUp(type);
    }
    internal SynapseTarget(SchematicConfiguration.TargetConfiguration configuration, SynapseSchematic schematic) : 
        this(configuration.TargetType, configuration.Position, configuration.Rotation,
            configuration.Scale)
    {
        Parent = schematic;
        schematic._targets.Add(this);

        OriginalScale = configuration.Scale;
        CustomAttributes = configuration.CustomAttributes;
    }

    private void SetUp(TargetType type)
    {
        Map._synapseTargets.Add(this);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;

        SynapseTargetType = type;
    }
    private ShootingTarget CreateTarget(TargetType type, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        var ot = Object.Instantiate(Prefabs[type], position, rotation);

        var transform = ot.transform;
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;
        ot.NetworkScale = scale;

        NetworkServer.Spawn(ot.gameObject);
        return ot;
    }
    
    public enum TargetType
    {
        Sport,
        DBoy,
        Binary
    }
}