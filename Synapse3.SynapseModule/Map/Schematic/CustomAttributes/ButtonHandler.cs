using System;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Map.Schematic.CustomAttributes;

public class ButtonHandler : AttributeHandler
{
    private readonly SynapseObjectEvents _synapseObjectEvents;
    
    public override string Name => "Button";

    public ButtonHandler(PlayerEvents playerEvents, SynapseObjectEvents synapseObjectEvents)
    {
        _synapseObjectEvents = synapseObjectEvents;
        playerEvents.DoorInteract.Subscribe(Door);
    }

    public override void OnLoad(ISynapseObject synapseObject, ArraySegment<string> args)
    {
        var id = 0u;
        if (args.Count > 0 && uint.TryParse(args.At(0), out var newId)) id = newId;
        synapseObject.ObjectData["button"] = id;
    }

    private void Door(DoorInteractEvent ev)
    {
        try
        {
            if(!SynapseObjects.Contains(ev.Door) || !ev.Door.ObjectData.ContainsKey("button")) return;
            if(ev.Door.ObjectData["button"] is not uint id) return;

            var evButton = new ButtonPressedEvent(ev.Door, id, ev.Player);
            _synapseObjectEvents.ButtonPressed.Raise(evButton);

            ev.LockBypassRejected = false;
            ev.PlayDeniedSound = false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Events: Button Pressed Event failed:\n" + ex);
        }
    }
}