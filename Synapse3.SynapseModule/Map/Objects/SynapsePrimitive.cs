using AdminToys;
using Mirror;
using Synapse3.SynapseModule.Map.Schematic;
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
        ToyBase = CreatePrimitive(primitiveType, color, Vector3.positiveInfinity, rotation, scale);
        SetUp();
    }
    
    private void SetUp()
    {
        Map._synapsePrimitives.Add(this);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;
    }
    private PrimitiveObjectToy CreatePrimitive(PrimitiveType primitiveType, Color color, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        var ot = Object.Instantiate(Prefab, position, rotation);
        NetworkServer.Spawn(ot.gameObject);
        ot.NetworkPrimitiveType = primitiveType;
        ot.NetworkMaterialColor = color;
        var transform = ot.transform;
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;
        ot.NetworkScale = scale;

        return ot;
    }
}