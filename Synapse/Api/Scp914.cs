using Mirror;
using Scp914;
using UnityEngine;

namespace Synapse.Api
{
    public class Scp914
    {
        internal Scp914() { }

        public Scp914Knob KnobState
        {
            get => Scp914Machine.singleton.knobState;
            set => Scp914Machine.singleton.SetKnobState(value);
        }

        public GameObject GameObject => Scp914Machine.singleton.gameObject;

        public bool IsActive => Scp914Machine.singleton.working;

        public Transform Intake
        {
            get => Scp914Machine.singleton.intake;
            set => Scp914Machine.singleton.intake = value;
        }

        public Transform Output
        {
            get => Scp914Machine.singleton.output;
            set => Scp914Machine.singleton.output = value;
        }
        
        public void Activate() => Scp914Machine.singleton.RpcActivate(NetworkTime.time);
    }
}
