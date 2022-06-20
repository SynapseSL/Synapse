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
    }
    
    public int ID { get; private set; }

    public SynapseCustomObject(Vector3 position, Quaternion rotation, Vector3 scale, int id)
    {
        GameObject = CreateObject(position, rotation, scale, id);
        SetUp(id);
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