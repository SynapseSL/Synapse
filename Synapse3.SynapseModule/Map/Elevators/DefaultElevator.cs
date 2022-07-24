using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CustomPlayerEffects;
using LightContainmentZoneDecontamination;
using MEC;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Elevators;

public abstract class DefaultElevator : IElevator
{
    public abstract string Name { get; }
    public abstract int Id { get; }
    public abstract bool Locked { get; set; }
    public abstract bool IsMoving { get; }
    public IElevatorDestination CurrentDestination { get; set; }
    public abstract ReadOnlyCollection<IElevatorDestination> Destinations { get; }

    public abstract void MoveToDestination(int destinationId);

    public void MoveToNext()
    {
        if(CurrentDestination == null) return;
        
        for (int i = 0; i < Destinations.Count; i++)
        {
            var destination = Destinations[i];
            if (CurrentDestination == destination)
            {
                if (i >= Destinations.Count - 1)
                {
                    MoveToDestination(Destinations.First().ElevatorId);
                    return;
                }

                MoveToDestination(Destinations[i + 1].ElevatorId);
                return;
            }
        }
    }

    public void MoveContent(int destinationId)
    {
        try
        {
            var destination = GetDestination(destinationId);

        var ev = new ElevatorMoveContentEvent(this)
        {
            Destination = destination
        };
        try
        {
            Synapse.Get<MapEvents>().ElevatorMoveContent.Raise(ev);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Elevator Move Content Event failed\n" + ex);
        }
        destination = ev.Destination;

        foreach (var player in Synapse.Get<PlayerService>().Players)
        {
            //Move Players
            if (InsideAnyDestination(player.Position, destinationId, out var localPosition, out var transform))
            {
                var rotation = player.Rotation.eulerAngles.y - (transform.rotation.eulerAngles.y - destination.Transform.eulerAngles.y);
                player.PlayerMovementSync.AddSafeTime(0.5f);
                player.PlayerMovementSync.OverridePosition(destination.GetWorldPosition(localPosition),
                    new PlayerMovementSync.PlayerRotation(null, rotation));

                if (DecontaminationController.Singleton.IsDecontaminating)
                {
                    var y = player.transform.position.y;

                    if (y <= 200f || y >= -200f)
                        player.PlayerEffectsController.EnableEffect<Decontaminating>();
                }
            }
            
            //Move 106 Portal
            var script = player.Hub.scp106PlayerScript;
            if (script != null &&
                InsideAnyDestination(script.portalPosition, destinationId, out localPosition, out _))
            {
                script.SetPortalPosition(Vector3.zero, destination.GetWorldPosition(localPosition));
            }
        }
        
        //Move Tantrum
        foreach (var tantrum in TantrumEnvironmentalHazard.AllTantrums)
        {
            if (InsideAnyDestination(tantrum.transform.position, destinationId, out var localPosition, out _))
            {
                tantrum.SetTantrumPosition(Vector3.zero, destination.GetWorldPosition(localPosition));
            }
        }

        //Move SynapseObjects like Items/Ragdolls
        foreach (var otherDestination in Destinations)
        {
            if(destination == otherDestination) continue;
            var synapseObjects = new HashSet<ISynapseObject>();
            var box = otherDestination.RangeScale;
            box.y *= 1.5f;
            box.x *= 1.1f;
            box.z *= 1.1f;
            
            foreach (var collider in Physics.OverlapBox(otherDestination.DestinationPosition,box))
            {
                var synapse = collider.GetComponentInParent<SynapseObjectScript>();
                if(synapse == null) continue;
                if (synapse.Object is not DefaultSynapseObject { MoveInElevator: true }) continue;
                if (!synapseObjects.Add(synapse.Object)) continue;
                if (!InsideAnyDestination(synapse.Object.Position, destinationId, out var local, out var transform,
                        true))
                    continue;
                synapse.Object.Position = destination.GetWorldPosition(local);
                synapse.Object.Rotation =
                    Quaternion.Euler(
                        destination.Transform.TransformVector(
                            transform.InverseTransformVector(synapse.Object.Rotation.eulerAngles)));
            }
        }

        if (ev.OpenDoorManually)
            Timing.CallDelayed(ev.OpenManuallyDelay, () => destination.Open = true);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 API: Elevator Move Content failed\n" + ex);
        }
    }
    
    public IElevatorDestination GetDestination(int id) => Destinations.FirstOrDefault(x => x.ElevatorId == id);

    private bool InsideAnyDestination(Vector3 position, int goalDestination, out Vector3 localPosition,
        out Transform transform,bool synapseObject = false)
    {
        foreach (var destination in Destinations)
        {
            if (destination.ElevatorId == goalDestination) continue;

            var destinationPos = destination.DestinationPosition;

            //Everything that is inside the Collider Box doesn't need a second check and just causes problem with some items like keycards on the ground
            if (!synapseObject)
            {
                if (Mathf.Abs(destinationPos.x - position.x) > destination.RangeScale.x) continue;
                if (Mathf.Abs(destinationPos.y - position.y) > destination.RangeScale.y * 1.5) continue;
                if (Mathf.Abs(destinationPos.z - position.z) > destination.RangeScale.z) continue;   
            }

            localPosition = destination.GetLocalPosition(position);
            transform = destination.Transform;
            return true;
        }

        transform = null;
        localPosition = Vector3.zero;
        return false;
    }
}