﻿using System.Collections.Generic;
using Mirror;
using RelativePositioning;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Schematic;

public abstract class DefaultSynapseObject : ISynapseObject
{
    protected DefaultSynapseObject()
    {
        Map = Synapse.Get<MapService>();
        Map._synapseObjects.Add(this);
    }

    protected MapService Map { get; }
    
    public abstract GameObject GameObject { get; }
    public abstract ObjectType Type { get; }

    public Dictionary<string, object> ObjectData { get; set; } = new ();
    public List<string> CustomAttributes { get; set; }

    public Vector3 OriginalScale { get; internal set; }

    public virtual bool MoveInElevator { get; set; } = false;

    private ISynapseObject _parent;
    public ISynapseObject Parent
    {
        get => _parent;
        set
        {
            _parent = value;
            if (GameObject != null)
                GameObject.transform.parent = value?.GameObject?.transform;
        }
    }

    public ISynapseObject RootParent
    {
        get
        {
            var parent = Parent;
            if (parent == null) return null;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }

            return parent;
        }
    }

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
    
    public void DeleteWayPoints()
    {
        var waypoints = GameObject.GetComponentsInChildren<WaypointBase>();
        foreach (var waypoint in waypoints)
        {
            Object.Destroy(waypoint);
        }
    }

    public void DisbleWayPoints()
    {
        var waypoints = GameObject.GetComponentsInChildren<WaypointBase>();
        foreach (var waypoint in waypoints)
        {
            WaypointBase.SetWaypoints[waypoint._id] = false;
        }
    }
}