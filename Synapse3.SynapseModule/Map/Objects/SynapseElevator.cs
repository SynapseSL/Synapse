using Synapse3.SynapseModule.Enums;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapseElevator
{
    internal SynapseElevator(Lift lift) => Lift = lift;
    internal Lift Lift { get; }

    /// <summary>
    /// The gameObject of the elevator
    /// </summary>
    public GameObject GameObject => Lift.gameObject;

    /// <summary>
    /// The name of the elevator
    /// </summary>
    public string Name => Lift.elevatorName;

    /// <summary>
    /// The position of the elevator
    /// </summary>
    public Vector3 Position => GameObject.transform.position;

    /// <summary>
    /// The current status of the elevator
    /// </summary>
    public Lift.Status Status
    {
        get => Lift.status;
        set => Lift.SetStatus((byte)value);
    }

    /// <summary>
    /// True if the elevator is locked
    /// </summary>
    public bool Locked
    {
        get => Lift._locked;
        set => Lift.SetLock(Locked, value);
    }

    /// <summary>
    /// True if the elevator can be used by a player
    /// </summary>
    public bool Operative => Lift.operative;

    //TODO: WTF?!?
    public float MaxDistance
    {
        get => Lift.maxDistance;
        set => Lift.maxDistance = value;
    }

    /// <summary>
    /// Manually activate the elevator
    /// </summary>
    public bool Use() => Lift.UseLift();

    /// <summary>
    /// The type of the elevator
    /// </summary>
    public ElevatorType ElevatorType
    {
        get
        {
            return Name switch
            {
                "GateB" => ElevatorType.GateB,
                "GateA" => ElevatorType.GateA,
                "SCP-049" => ElevatorType.Scp049,
                "ElA" => ElevatorType.ElALeft,
                "ElA2" => ElevatorType.ElARight,
                "ElB" => ElevatorType.ElBLeft,
                "ElB2" => ElevatorType.ElBRight,
                _ => ElevatorType.None,
            };
        }
    }

    public override string ToString() => Name;
}