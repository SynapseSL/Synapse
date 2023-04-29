using Neuron.Core.Meta;
using Synapse3.SynapseModule.Map.Elevators;
using Synapse3.SynapseModule.Map.Rooms;
using UnityEngine;

namespace Synapse3.ExamplePlugin;

[Automatic]
[Elevator(
    Name = "Test",
    Id = 99,
    ChamberSchematicId = 50,
    DestinationSchematicId = 51
)]
public class ExampleElevator : CustomElevator
{
    protected override void OnGenerate()
    {
        Logger.Warn("GENERATE ELEVATOR!");
        SpawnDestination(new RoomPoint(RoomType.Surface.ToString(), new(18.92f,-8.35f,-42.84f), Vector3.zero));
        SpawnDestination(new RoomPoint(RoomType.Surface.ToString(), new (18.92f,-8.35f + 10f,-42.84f), Vector3.zero));
        SpawnDestination(new RoomPoint(RoomType.Surface.ToString(), new (18.92f,-8.35f + 20f,-42.84f), Vector3.zero));
    }

    public override float MoveTime => 5f;
}

/*
public class ElevatorEventHandler
{
    public ElevatorEventHandler(PlayerEvents playerEvents, RoundEvents roundEvents)
    {
        playerEvents.DoorInteract.Subscribe(DoorInteract);
        roundEvents.Start.Subscribe(CreateElevator);
    }

    private void DoorInteract(DoorInteractEvent ev)
    {
        if (!ev.Door.ObjectData.ContainsKey("elev")) return;
        
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

    private void CreateElevator(RoundStartEvent _)
    {
        new ExampleElevator();
    }
}*/