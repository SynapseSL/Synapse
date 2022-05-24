using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Api
{
    public class Elevator
    {
        internal Elevator(Lift lift) => Lift = lift;

        internal Lift Lift { get; }

        public GameObject GameObject => Lift.gameObject;

        public string Name => Lift.elevatorName;

        public Vector3 Position => GameObject.transform.position;

        public Lift.Status Status { get => Lift.status; set => Lift.SetStatus((byte)value); }

        public bool Locked { get => Lift._locked; set => Lift.SetLock(Locked, value); }

        public bool Operative => Lift.operative;

        public float MaxDistance { get => Lift.maxDistance; set => Lift.maxDistance = value; }

        public bool Use() => Lift.UseLift();

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
}
