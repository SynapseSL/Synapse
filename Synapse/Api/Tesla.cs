using UnityEngine;

namespace Synapse.Api
{
    public class Tesla
    {
        internal Tesla(TeslaGate gate) => Gate = gate;

        private readonly TeslaGate Gate;

        public GameObject GameObject => Gate.gameObject;

        public Vector3 Position { get => Gate.localPosition; }

        public void Trigger() => Gate.RpcPlayAnimation();

        public void InstantTrigger() => Gate.RpcInstantTesla();

        public float SizeOfTrigger { get => Gate.sizeOfTrigger; set => Gate.sizeOfTrigger = value; }
    }
}
