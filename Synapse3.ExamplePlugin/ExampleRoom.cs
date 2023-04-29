﻿using Neuron.Core.Logging;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Rooms;
using UnityEngine;

namespace Synapse3.ExamplePlugin;

//[Automatic]
[CustomRoom(
    Name = "ExampleRoom",
    Id = 99,
    SchematicId = 1
    )]
public class ExampleRoom : SynapseCustomRoom
{
    public override uint Zone => (int)ZoneType.Surface;

    public override void OnGenerate()
    {
        NeuronLogger.For<ExamplePlugin>().Warn("Generated Example Room!");
    }
}

public class RoomEventHandler
{
    private readonly RoomService _roomService;

    public RoomEventHandler(RoundEvents roundEvents, RoomService roomService)
    {
        _roomService = roomService;
        
        roundEvents.Start.Subscribe(OnStart);
    }

    private void OnStart(RoundStartEvent _)
    {
        _roomService.SpawnCustomRoom(99, Vector3.up * 1100f);
    }
}