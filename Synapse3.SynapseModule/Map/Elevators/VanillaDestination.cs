using UnityEngine;

namespace Synapse3.SynapseModule.Map.Elevators;

public class VanillaDestination : IElevatorDestination
{
    public VanillaDestination(SynapseElevator elevator, Lift.Elevator dest, string name, int id)
    {
        VanillaElevator = dest;
        Elevator = elevator;
        Transform = dest.target;
        DestinationName = name;
        ElevatorId = id;
        RangeScale = elevator.Lift.maxDistance * Vector3.one;
    }
    
    public Lift.Elevator VanillaElevator { get; }
    
    public IElevator Elevator { get; }
    public Transform Transform { get; }
    public string DestinationName { get; }

    public bool Open
    {
        get
        {
            switch (ElevatorId)
            {
                case 0: return ((SynapseElevator)Elevator).Lift.status == Lift.Status.Up;
                case 1: return ((SynapseElevator)Elevator).Lift.status == Lift.Status.Down;
            }

            return false;
        }
        set
        {
            var elevator = ((SynapseElevator)Elevator);
            switch (ElevatorId)
            {
                case 0:
                    var otherOpen = elevator.Destinations[1].Open;

                    if (value)
                        elevator.Lift.SetStatus(0);
                    else
                        elevator.Lift.SetStatus(otherOpen ? (byte)1 : (byte)2);
                    break;
                case 1:
                    otherOpen = elevator.Destinations[0].Open;
                    
                    if (value)
                        elevator.Lift.SetStatus(1);
                    else
                        elevator.Lift.SetStatus(otherOpen ? (byte)0 : (byte)2);
                    break;
            }
        }
    }

    public bool Locked
    {
        get => ((SynapseElevator)Elevator).Locked;
        set => ((SynapseElevator)Elevator).Locked = value;
    }
    
    public int ElevatorId { get; }
    public Vector3 RangeScale { get; set; }

    public Vector3 DestinationPosition
    {
        get => Transform.position;
        set => Transform.position = value;
    }
    

    public Vector3 GetWorldPosition(Vector3 localPosition) => Transform.TransformPoint(localPosition);
    public Vector3 GetLocalPosition(Vector3 worldPosition) => Transform.InverseTransformPoint(worldPosition);
}