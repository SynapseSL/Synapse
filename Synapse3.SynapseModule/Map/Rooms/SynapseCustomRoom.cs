using Neuron.Core.Logging;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Rooms;

public abstract class SynapseCustomRoom : DefaultSynapseObject ,IRoom
{
    public SynapseSchematic RoomSchematic { get; private set; }

    private GameObject _gameObject;
    public override GameObject GameObject => _gameObject;
    public override ObjectType Type => ObjectType.Room;

    public override Vector3 Position
    {
        get => RoomSchematic.Position;
        set => RoomSchematic.Position = value;
    }

    public override Quaternion Rotation
    {
        get => RoomSchematic.Rotation;
        set => RoomSchematic.Rotation = value;
    }

    public override Vector3 Scale
    {
        get => RoomSchematic.Scale;
        set => RoomSchematic.Scale = value;
    }


    public abstract string Name { get; }
    public abstract int ID { get; }
    public abstract int Zone { get; }
    public abstract int SchematicID { get; }
    
    
    public virtual void OnGenerate() { }
    public virtual void OnDespawn() { }


    public void Generate(Vector3 position)
    {
        if(RoomSchematic != null) return;
        
        _gameObject = new GameObject(Name);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;
        
        RoomSchematic = Synapse.Get<SchematicService>().SpawnSchematic(SchematicID, position);
        RoomSchematic.Parent = this;
        
        Synapse.Get<RoomService>()._rooms.Add(this);
        OnGenerate();
    }

    public override void Destroy()
    {
        Object.Destroy(GameObject);
    }

    public void Despawn()
    {
        Object.Destroy(GameObject);
    }

    public override void OnDestroy()
    {
        OnDespawn();
        Synapse.Get<RoomService>()._rooms.Remove(this);
        base.OnDestroy();
    }

    public void TurnOffLights(float duration)
    {
        foreach (var light in RoomSchematic.Lights)
        {
            light.ToyBase.netIdentity.DespawnForAllPlayers();
        }

        MEC.Timing.CallDelayed(duration, () =>
        {
            foreach (var light in RoomSchematic.Lights)
            {
                light.ToyBase.netIdentity.UpdatePositionRotationScale();
            }
        });
    }
}