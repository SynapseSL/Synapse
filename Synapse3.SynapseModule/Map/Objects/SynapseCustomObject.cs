using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapseCustomObject : DefaultSynapseObject
{
    public override GameObject GameObject { get; }
    public override ObjectType Type => ObjectType.Custom;
    public override void Destroy()
        => Object.Destroy(GameObject);
    public override void OnDestroy()
    {
        Map._synapseCustomObjects.Remove(this);
        base.OnDestroy();
        
        if (Parent is SynapseSchematic schematic) schematic._custom.Remove(this);
    }
    
    public int ID { get; private set; }

    public SynapseCustomObject(Vector3 position, Quaternion rotation, Vector3 scale, int id)
    {
        GameObject = CreateObject(position, rotation, scale, id);
        SetUp(id);
    }

    internal SynapseCustomObject(SchematicConfiguration.CustomObjectConfiguration configuration,
        SynapseSchematic schematic) :
        this(configuration.Position, configuration.Rotation, configuration.Scale, configuration.ID)
    {
        Parent = schematic;
        schematic._custom.Add(this);

        OriginalScale = configuration.Scale;
        CustomAttributes = configuration.CustomAttributes;
    }
    private void SetUp(int id)
    {
        ID = id;
        Map._synapseCustomObjects.Add(this);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;
    }
    private GameObject CreateObject(Vector3 position, Quaternion rotation, Vector3 scale, int id)
    {
        var gameObject = new GameObject("SynapseCustomObject-" + id)
        {
            transform =
            {
                position = position,
                rotation = rotation,
                localScale = scale
            }
        };
        return gameObject;
    }
}