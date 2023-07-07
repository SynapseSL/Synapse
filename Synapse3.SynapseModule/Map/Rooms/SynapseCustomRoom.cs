using MEC;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Rooms;

public abstract class SynapseCustomRoom : DefaultSynapseObject, IRoom, IHideable
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
    /// <summary>
    /// If set other than -1 or 0, the coin will disappear if the player is too far away
    /// </summary>
    public virtual float VisibleDistance => -1;

    /// <summary>
    /// Update frequency in seconds to check player for <see cref="VisibleDistance"/>
    /// </summary>
    public virtual float UpdateFrequencyVisble => -1;

    public ReadOnlyCollection<SynapseDoor> Doors => RoomSchematic.Doors;

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

        var roomService = Synapse.Get<RoomService>();
        roomService._rooms.Add(this);
        roomService._customRooms.Add(this);
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
        var roomService = Synapse.Get<RoomService>();
        roomService._rooms.Remove(this);
        roomService._customRooms.Remove(this);
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

    public virtual Color RoomColor
    {
        get => RoomSchematic.Lights.FirstOrDefault()?.LightColor ?? default;
        set
        {
            foreach (var light in RoomSchematic.Lights)
            {
                light.LightColor = value;
            }
        }
    }
    
    public void HideFromAll() => RoomSchematic.HideFromAll();

    public void ShowAll() => RoomSchematic.ShowAll();

    public void HideFromPlayer(SynapsePlayer player) => RoomSchematic.HideFromPlayer(player);

    public void ShowPlayer(SynapsePlayer player) => RoomSchematic.ShowPlayer(player);
}