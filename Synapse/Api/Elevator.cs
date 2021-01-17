using UnityEngine;
using Synapse.Api.Enum;

namespace Synapse.Api
{
    public class Elevator
    {
        internal Elevator(Lift lift) => Lift = lift;

        private Lift Lift;

        public GameObject GameObject => Lift.gameObject;

        public string Name => Lift.elevatorName;

        public Vector3 Position => GameObject.transform.position;

        public Lift.Status Status { get => Lift.status; set => Lift.SetStatus((byte)value); }

        public bool Locked { get => Lift._locked; set => Lift.SetLock(value); }

        public float MaxDistance { get => Lift.maxDistance; set => Lift.maxDistance = value; }

        public void Use() => Lift.UseLift();

        public ElevatorType ElevatorType
        {
            get
            {
                switch (Name)
                {
                    case "GateB": return ElevatorType.GateB;
                    case "GateA": return ElevatorType.GateA;
                    case "SCP-049": return ElevatorType.Scp049;
                    case "ElA": return ElevatorType.ElALeft;
                    case "ElA2": return ElevatorType.ElARight;
                    case "ElB": return ElevatorType.ElBLeft;
                    case "ElB2": return ElevatorType.ElBRight;
                    default: return ElevatorType.None;
                }
            }
        }

        public override string ToString() => Name;
    }
}
