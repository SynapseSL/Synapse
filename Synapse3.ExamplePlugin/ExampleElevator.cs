using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Neuron.Core.Logging;
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

        var range = Vector3.one * 3;
        range.y *= 1.5f;

        var schematic1 = service.SpawnSchematic(8, new Vector3(0f, 1000f, 2.5f), Quaternion.Euler(0f, 90f, 0f));
        var schematic2 = service.SpawnSchematic(8, new Vector3(0f, 1000f, -70f), Quaternion.Euler(0f, -90f, 0f));
        var schematic3 = service.SpawnSchematic(8, new Vector3(190f, 992.5f, -87), Quaternion.Euler(0f, -90f, 0f));

        list.Add(new SchematicDestination(schematic1, 0, "Gate-A", this, range));
        list.Add(new SchematicDestination(schematic2, 1, "Above Chaos", this, range));
        list.Add(new SchematicDestination(schematic3, 2, "MTF Spawn", this, range));

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

public class ElevatorEventHandler
{
    private readonly PlayerEvents _player;
    private readonly RoundEvents _round;
    
    public ElevatorEventHandler()
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
            
            
            var id = (uint)ev.Door.ObjectData["id"];

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