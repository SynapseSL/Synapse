using System.Linq;
using UnityEngine;

namespace Synapse.Api
{
    public class Tesla
    {
        internal Tesla(TeslaGate gate)
        {
            Gate = gate;
            Room = Server.Get.Map.Rooms.OrderBy(room => Vector3.Distance(room.Position, gate.localPosition)).First();
        }

        internal TeslaGate Gate { get; }

        public Room Room { get; internal set; }

        public GameObject GameObject => Gate.gameObject;

        public Vector3 Position { get => Gate.localPosition; }

        public void Trigger() => Gate.RpcPlayAnimation();

        public void InstantTrigger() => Gate.RpcInstantTesla();

        public float SizeOfTrigger { get => Gate.sizeOfTrigger; set => Gate.sizeOfTrigger = value; }
    }
}
