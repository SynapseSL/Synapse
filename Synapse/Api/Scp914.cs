using Scp914;
using UnityEngine;

namespace Synapse.Api
{
    public class Scp914
    {
        public static Scp914 Get
            => Map.Get.Scp914;

        internal Scp914() { }

        public Scp914Controller Scp914Controller { get; internal set; }

        public GameObject GameObject
            => Scp914Controller.gameObject;

        public Scp914KnobSetting KnobState
        {
            get => Scp914Controller._knobSetting;
            set => Scp914Controller.Network_knobSetting = value;
        }

        public bool IsActive
            => Scp914Controller._isUpgrading;

        public Transform Intake
        {
            get => Scp914Controller._intakeChamber;
            set => Scp914Controller._intakeChamber = value;
        }

        public Transform Output
        {
            get => Scp914Controller._outputChamber;
            set => Scp914Controller._outputChamber = value;
        }

        public void Activate()
            => Scp914Controller.ServerInteract(null, 0);
    }
}
