using MEC;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Rooms;

public abstract class SynapseCustomRoom : DefaultSynapseObject, IRoom
{
    public SynapseSchematic RoomSchematic { get; private set; }

    private GameObject _gameObject;
    public sealed override GameObject GameObject => _gameObject;
    public sealed override ObjectType Type => ObjectType.Room;

    public sealed override Vector3 Position
    {
        get => RoomSchematic.Position;
        set => RoomSchematic.Position = value;
    }

    public sealed override Quaternion Rotation
    {
        get => RoomSchematic.Rotation;
        set => RoomSchematic.Rotation = value;
    }

    public sealed override Vector3 Scale
    {
        get => RoomSchematic.Scale;
        set => RoomSchematic.Scale = value;
    }
    
    public CustomRoomAttribute Attribute { get; set; }

    public string Name => Attribute.Name;

    public uint Id => Attribute.Id;
    
    public abstract uint Zone { get; }

    public virtual void OnGenerate() { }
    public virtual void OnDeSpawn() { }
    public virtual void Load() { }
    
    public void Generate(Vector3 position)
    {
        if(RoomSchematic != null) return;
        
        _gameObject = new GameObject(Name);
        var comp = GameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;
        
        RoomSchematic = Synapse.Get<SchematicService>().SpawnSchematic(Attribute.SchematicId, position);
        RoomSchematic.Parent = this;
        
        Synapse.Get<RoomService>()._rooms.Add(this);
        OnGenerate();
    }

    public sealed override void Destroy()
    {
        Object.Destroy(GameObject);
    }

    public void DeSpawn()
    {
        Object.Destroy(GameObject);
    }

    public sealed override void OnDestroy()
    {
        OnDeSpawn();
        Synapse.Get<RoomService>()._rooms.Remove(this);
        base.OnDestroy();
    }

    public virtual void TurnOffLights(float duration)
    {
        foreach (var light in RoomSchematic.Lights)
        {
            light.ToyBase.netIdentity.UnSpawnForAllPlayers();
        }

        Timing.CallDelayed(duration, () =>
        {
            foreach (var light in RoomSchematic.Lights)
            {
                light.ToyBase.netIdentity.UpdatePositionRotationScale();
            }
        });
    }
    
    public sealed override void HideFromAll() => RoomSchematic.HideFromAll();

    public sealed override void ShowAll() => RoomSchematic.ShowAll();

    public sealed override void HideFromPlayer(SynapsePlayer player) => RoomSchematic.HideFromPlayer(player);

    public sealed override void ShowPlayer(SynapsePlayer player) => RoomSchematic.ShowPlayer(player);
}