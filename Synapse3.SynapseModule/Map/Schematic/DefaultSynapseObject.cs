using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Schematic;

public abstract class DefaultSynapseObject : ISynapseObject
{
    protected DefaultSynapseObject()
    {
        Map = Synapse.Get<MapService>();
        Map._synapseObjects.Add(this);
    }
    
    public MapService Map { get; }
    
    public abstract GameObject GameObject { get; }
    public abstract ObjectType Type { get; }

    public Dictionary<string, object> ObjectData { get; set; } = new Dictionary<string, object>();
    public List<string> CustomAttributes { get; set; }

    public Vector3 OriginalScale { get; internal set; }
    //public SynapseObject Parent { get; internal set; }
    //TODO: public SynapseItem ItemParent { get; internal set; }

    public virtual Vector3 Position
    {
        get => GameObject.transform.position;
        set => GameObject.transform.position = value;
    }

    public virtual Quaternion Rotation
    {
        get => GameObject.transform.rotation;
        set => GameObject.transform.rotation = value;
    }

    public virtual Vector3 Scale
    {
        get => GameObject.transform.localScale;
        set => GameObject.transform.localScale = value;
    }

    public virtual Rigidbody Rigidbody { get; set; }

    public virtual void ApplyPhysics()
    {
        if (GameObject.GetComponent<Rigidbody>() == null)
            Rigidbody = GameObject.AddComponent<Rigidbody>();
    }

    public virtual void RemoveParent()
    {
        GameObject.transform.parent = null;
    }

    public virtual void Destroy()
    {
        NetworkServer.Destroy(GameObject);
    }

    public virtual void OnDestroy()
    {
        Map._synapseObjects.Remove(this);
    }
}