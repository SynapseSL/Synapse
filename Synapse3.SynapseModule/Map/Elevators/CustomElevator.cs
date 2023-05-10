using System.Collections.Generic;
using Neuron.Core.Meta;
using Ninject;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Rooms;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Elevators;

public abstract class CustomElevator : InjectedLoggerBase, ICustomElevator
{
    [Inject] public MapEvents MapEvents { get; set; }

    protected readonly List<IElevatorDestination> _destinations = new();

    public uint ElevatorId => Attribute.Id;
    public IElevatorChamber Chamber { get; private set; }
    public IElevatorDestination[] Destinations => _destinations.ToArray();
    public IElevatorDestination CurrentDestination { get; private set; }

    public void MoveToDestination(uint destinationId)
    {
        if (Chamber.IsMoving) return;
        if (Destinations.Length == 0) return;
        if (Destinations.Length <= destinationId) destinationId = 0;
        var nextDestination = Destinations[destinationId];
        (Chamber as ICustomElevatorChamber)?.MoveToLocation(nextDestination.Position, nextDestination.Rotation);
        CurrentDestination = nextDestination;
    }

    public void MoveToNext()
    {
        var id = (uint)Destinations.IndexOf(CurrentDestination) + 1;
        if (id >= Destinations.Length) id = 0;
        MoveToDestination(id);
    }

    protected void SpawnDestination(RoomPoint point)
        => SpawnDestination(point.GetMapPosition(), point.GetMapRotation());

    public void SpawnDestination(Vector3 position, Quaternion rotation)
    {
        var destination = Synapse.Create<CustomElevatorDestination>(false);
        _destinations.Add(destination);
        destination.Generate(this, Attribute.DestinationSchematicId, position, rotation);
    }

    public void Generate()
    {
        OnGenerate();
        if (_destinations.Count == 0) return;
        var chamber = Synapse.Create<CustomElevatorChamber>(false);
        Chamber = chamber;
        chamber.Generate(this, Attribute.ChamberSchematicId,
            Destinations[0] as CustomElevatorDestination);
        CurrentDestination = Destinations[0];
    }

    public void Unload()
    {
        Chamber = null;
        _destinations.Clear();
    }

    public ElevatorAttribute Attribute { get; set; }

    public virtual void Load() { }

    protected abstract void OnGenerate();
    
    public abstract float MoveSpeed { get; }
}