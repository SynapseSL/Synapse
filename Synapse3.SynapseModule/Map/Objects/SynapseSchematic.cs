using System.Collections.Generic;
using System.Collections.ObjectModel;
using RelativePositioning;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapseSchematic : DefaultSynapseObject, IHideable, IRefreshable
{
    public override GameObject GameObject { get; }
    public override ObjectType Type => ObjectType.Schematic;
    public override Vector3 Position
    {
        set
        {
            base.Position = value;
            UpdatePositionAndRotation();
        }
    }
    public override Quaternion Rotation
    {
        set
        {
            base.Rotation = value;
            UpdatePositionAndRotation();
        }
    }
    public override Vector3 Scale
    {
        set
        {
            base.Scale = value;
            UpdateScale();
        }
    }
    public override void Destroy()
    {
        Object.Destroy(GameObject);
    }

    public override void OnDestroy()
    {
        Map._synapseSchematics.Remove(this);
        base.OnDestroy();
    }

    public void HideFromAll()
    {
        foreach (var child in _children)
        {
            if (child is IHideable hideable)
                hideable.HideFromAll();
        }
    }

    public void ShowAll()
    {
        foreach (var child in _children)
        {
            if (child is IHideable hideable)
                hideable.ShowAll();
        }
    }

    public void HideFromPlayer(SynapsePlayer player)
    {
        foreach (var child in _children)
        {
            if (child is IHideable hideable)
                hideable.HideFromPlayer(player);
        }
    }

    public void ShowPlayer(SynapsePlayer player)
    {
        foreach (var child in _children)
        {
            if (child is IHideable hideable)
                hideable.ShowPlayer(player);
        }
    }


    private readonly List<ISynapseObject> _children = new ();
    internal readonly List<SynapsePrimitive> _primitives = new ();
    internal readonly List<SynapseLight> _lights = new ();
    internal readonly List<SynapseTarget> _targets = new();
    internal readonly List<SynapseWorkStation> _workStations = new();
    internal readonly List<SynapseDoor> _doors = new();
    internal readonly List<SynapseCustomObject> _custom = new();
    internal readonly List<SynapseRagDoll> _ragdolls = new ();
    internal readonly List<SynapseGenerator> _generators = new();
    internal readonly List<SynapseLocker> _lockers = new();
    internal readonly List<SynapseItem> _items = new();
    internal readonly List<SynapseDummy> _dummies = new();

    public ReadOnlyCollection<ISynapseObject> Children => _children.AsReadOnly();
    public ReadOnlyCollection<SynapsePrimitive> Primitives => _primitives.AsReadOnly();
    public ReadOnlyCollection<SynapseLight> Lights => _lights.AsReadOnly();
    public ReadOnlyCollection<SynapseTarget> Targets => _targets.AsReadOnly();
    public ReadOnlyCollection<SynapseWorkStation> WorkStations => _workStations.AsReadOnly();
    public ReadOnlyCollection<SynapseDoor> Doors => _doors.AsReadOnly();
    public ReadOnlyCollection<SynapseCustomObject> CustomObjects => _custom.AsReadOnly();
    public ReadOnlyCollection<SynapseRagDoll> RagDolls => _ragdolls.AsReadOnly();
    public ReadOnlyCollection<SynapseGenerator> Generators => _generators.AsReadOnly();
    public ReadOnlyCollection<SynapseLocker> Lockers => _lockers.AsReadOnly();
    public ReadOnlyCollection<SynapseItem> Items => _items.AsReadOnly();
    public ReadOnlyCollection<SynapseDummy> Dummies => _dummies.AsReadOnly();

    public string Name { get; }

    public uint ID { get; }

    public SynapseSchematic(SchematicConfiguration configuration)
    {
        Name = configuration.Name;
        ID = configuration.Id;
        GameObject = new GameObject(Name);
        
        //Add children
        foreach (var primitive in configuration.Primitives)
        {
            _children.Add(new SynapsePrimitive(primitive, this));
        }

        foreach (var light in configuration.Lights)
        {
            _children.Add(new SynapseLight(light, this));
        }

        foreach (var target in configuration.Targets)
        {
            _children.Add(new SynapseTarget(target, this));
        }

        foreach (var station in configuration.WorkStations)
        {
            _children.Add(new SynapseWorkStation(station, this));
        }

        foreach (var door in configuration.Doors)
        {
            _children.Add(new SynapseDoor(door, this));
        }

        foreach (var rag in configuration.Ragdolls)
        {
            _children.Add(new SynapseRagDoll(rag, this));
        }

        foreach (var locker in configuration.Lockers)
        {
            _children.Add(new SynapseLocker(locker, this));
        }

        foreach (var gen in configuration.Generators)
        {
            _children.Add(new SynapseGenerator(gen, this));
        }

        foreach (var customObject in configuration.CustomObjects)
        {
            _children.Add(new SynapseCustomObject(customObject,this));
        }

        foreach (var item in configuration.Items)
        {
            _children.Add(new SynapseItem(item, this));
        }

        foreach (var dummy in configuration.Dummies)
        {
            _children.Add(new SynapseDummy(dummy, this));
        }

        Map._synapseSchematics.Add(this);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;

        var waypoints = GameObject.GetComponentsInChildren<WaypointBase>();
        foreach (var waypoint in waypoints)
        {
            SynapseLogger<SynapseSchematic>.Warn("Found Waypoint in Schematic ... deleting");
            Object.Destroy(waypoint);
        }
    }
    
    private void UpdatePositionAndRotation()
    {
        foreach (var child in Children)
        {
            if (child is not IRefreshable refresh) continue;
            
            refresh.Refresh();
        }
    }
    private void UpdateScale()
    {
        foreach (var child in Children)
        {
            if (child is not DefaultSynapseObject defaultObject) continue;

            child.Scale = new Vector3(defaultObject.OriginalScale.x * Scale.x, defaultObject.OriginalScale.y * Scale.y,
                defaultObject.OriginalScale.z * Scale.z);
        }
    }

    public void Refresh()
    {
        UpdatePositionAndRotation();
        UpdateScale();
    }

    public bool Update { get; set; } = false;
    public float UpdateFrequency { get; set; }
}