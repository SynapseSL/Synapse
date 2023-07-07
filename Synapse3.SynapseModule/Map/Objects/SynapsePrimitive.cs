using AdminToys;
using Mirror;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapsePrimitive : SynapseToyObject<PrimitiveObjectToy>
{
    public static PrimitiveObjectToy Prefab { get; internal set; }

    public override PrimitiveObjectToy ToyBase { get; }
    public override ObjectType Type => ObjectType.Primitive;
    public override void OnDestroy()
    {
        Map._synapsePrimitives.Remove(this);
        base.OnDestroy();

        if (Parent is SynapseSchematic schematic) schematic._primitives.Remove(this);
    }

    public Color Color
        => ToyBase.MaterialColor;

    public PrimitiveType PrimitiveType
        => ToyBase.PrimitiveType;

    public float SyncInterval
    {
        get => ToyBase.syncInterval;
        set => ToyBase.syncInterval = value;
    }

    
    public SynapsePrimitive(PrimitiveType primitiveType, Color color, Vector3 position, Quaternion rotation,
        Vector3 scale)
    {
        ToyBase = CreatePrimitive(primitiveType, color, position, rotation, scale);
        SetUp();
    }

    internal SynapsePrimitive(SchematicConfiguration.PrimitiveConfiguration configuration, SynapseSchematic schematic) :
        this(configuration.PrimitiveType, configuration.Color, configuration.Position, configuration.Rotation,
            configuration.Scale)
    {
        Parent = schematic;
        schematic._primitives.Add(this);

        OriginalScale = configuration.Scale;
        CustomAttributes = configuration.CustomAttributes;
        
        if(configuration.Physics)
            ApplyPhysics();
    }
    private void SetUp()
    {
        Map._synapsePrimitives.Add(this);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;
    }
    private PrimitiveObjectToy CreatePrimitive(PrimitiveType primitiveType, Color color, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        var objectToy = Object.Instantiate(Prefab, position, rotation);
        NetworkServer.Spawn(objectToy.gameObject);
        objectToy.NetworkPrimitiveType = primitiveType;
        objectToy.NetworkMaterialColor = color;
        var transform = objectToy.transform;
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;
        objectToy.NetworkScale = scale;

        return objectToy;
    }
}