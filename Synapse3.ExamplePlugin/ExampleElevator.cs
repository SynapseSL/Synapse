using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Synapse3.SynapseModule;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Elevators;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.ExamplePlugin;

public class ExampleElevator : CustomElevator
{
    public ExampleElevator()
    {
        var list = new List<IElevatorDestination>();
        var service = Synapse.Get<SchematicService>();

        list.Add(new SchematicDestination(service.SpawnSchematic(8, new Vector3(0f, 1000f, 0f)), 0, "First", this,Vector3.one * 3));
        list.Add(new SchematicDestination(service.SpawnSchematic(8, new Vector3(0f, 1000f, -50f)), 1, "First", this,Vector3.one * 3));
        list.Add(new SchematicDestination(service.SpawnSchematic(8, new Vector3(170f, 992.5f, -60)), 2, "First", this,Vector3.one * 3));

        foreach (var destination in list)
        {
            if (destination.ElevatorId == 0)
                destination.Open = true;
            
            var schematicDestination = (SchematicDestination)destination;
            schematicDestination.Schematic.Doors.First().ObjectData["elev"] = this;
            schematicDestination.Schematic.Doors.First().ObjectData["id"] = destination.ElevatorId;
        }
        
        Destinations = list.AsReadOnly();
        CurrentDestination = Destinations[0];
    }
    
    public override ReadOnlyCollection<IElevatorDestination> Destinations { get; }
}

public class EventHandler
{
    private readonly PlayerEvents _player;
    private readonly RoundEvents _round;
    
    public EventHandler()
    {
        _player = Synapse.Get<PlayerEvents>();
        _round = Synapse.Get<RoundEvents>();
    }

    public void HookEvents()
    {
        _player.DoorInteract.Subscribe(OnDoor);
        _round.Start.Subscribe(CreateElevator);
    }

    public void UnHookEvents()
    {
        _player.DoorInteract.Unsubscribe(OnDoor);
        _round.Start.Unsubscribe(CreateElevator);
    }

    private void OnDoor(DoorInteractEvent ev)
    {
        if (ev.Door.ObjectData.ContainsKey("elev"))
        {
            var elevator = (ExampleElevator)ev.Door.ObjectData["elev"];
            var id = (int)ev.Door.ObjectData["id"];

            if (elevator.CurrentDestination.ElevatorId == id)
            {
                elevator.MoveToNext();
            }
            else
            {
                elevator.MoveToDestination(id);
            }
        }
    }

    private void CreateElevator(RoundStartEvent _)
    {
        new ExampleElevator();
    }
}