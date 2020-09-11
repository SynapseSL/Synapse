using UnityEngine;

namespace Synapse.Api.Components
{
    public class Tesla
    {
        internal Tesla(TeslaGate gate) => Gate = gate;

        private TeslaGate Gate;

        public bool Active { get; set; }

        public Vector3 Position { get => Gate.localPosition; }

        public void Trigger() => Gate.RpcPlayAnimation();

        public void InstantTrigger() => Gate.RpcInstantTesla();

        public float SizeOfTrigger { get => Gate.sizeOfTrigger; set => Gate.sizeOfTrigger = value; }
    }
}
