using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Rooms;
using UnityEngine;

namespace Synapse3.ExamplePlugin;

[Automatic]
[CustomRoom(
    Name = "ExampleRoom",
    Id = 99,
    SchematicId = 1
    )]
public class ExampleRoom : SynapseCustomRoom
{
    public override int Zone => (int)ZoneType.Surface;

    public override void OnGenerate()
    {
        NeuronLogger.For<ExamplePlugin>().Warn("Generated Example Room!");
    }
}

public class RoomEventHandler
{
    private RoundEvents _round;
    private RoomService _roomService;
    
    public RoomEventHandler()
    {
        _round = Synapse.Get<RoundEvents>();
        _roomService = Synapse.Get<RoomService>();
    }

    public void HookEvents()
    {
        _round.Start.Subscribe(OnStart);
    }

    public void UnHookEvents()
    {
        _round.Start.Unsubscribe(OnStart);
    }

    private void OnStart(RoundStartEvent _)
    {
        _roomService.SpawnCustomRoom(99, Vector3.up * 1100f);
    }
}