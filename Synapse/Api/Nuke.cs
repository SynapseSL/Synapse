using UnityEngine;

namespace Synapse.Api
{
    public class Nuke
    {
        public static Nuke Get => Map.Get.Nuke;

        internal Nuke() { }

        private AlphaWarheadController WarheadController => AlphaWarheadController.Host;

        public NukeInsidePanel InsidePanel { get; } = new NukeInsidePanel();

        public NukeOutsidePanel OutsidePanel { get; } = new NukeOutsidePanel();

        public float CountdownTime
        {
            get => WarheadController.NetworktimeToDetonation;
            set => WarheadController.NetworktimeToDetonation = value;
        }

        public int NukeKills => WarheadController.warheadKills;

        public bool Active => WarheadController.NetworkinProgress;

        public bool Detonated => WarheadController.detonated;

        public bool CanDetoante => WarheadController.CanDetonate;

        public void StartDetonation() => WarheadController.StartDetonation();

        public void CancelDetonation() => WarheadController.CancelDetonation();

        public void Detonate() => WarheadController.Detonate();

        public void InstantPrepare() => WarheadController.InstantPrepare();

        public void Shake() => WarheadController.RpcShake(false);

        public class NukeInsidePanel
        {
            public static NukeInsidePanel Get => Nuke.Get.InsidePanel;

            internal NukeInsidePanel() { }

            private AlphaWarheadNukesitePanel Panel => AlphaWarheadOutsitePanel.nukeside;

            public bool Enabled
            {
                get => Panel.Networkenabled;
                set => Panel.Networkenabled = value;
            }

            public float LeverStatus
            {
                get => Panel._leverStatus;
                set => Panel._leverStatus = value;
            }

            //Is used by a Harmony Patch
            public bool Locked { get; set; }

            public Transform Lever => Panel.lever;
        }

        public class NukeOutsidePanel
        {
            public static NukeOutsidePanel Get => Nuke.Get.OutsidePanel;

            internal NukeOutsidePanel() { }

            private AlphaWarheadOutsitePanel Panel => Server.Get.Host.GetComponent<AlphaWarheadOutsitePanel>();

            public bool KeyCardEntered
            {
                get => Panel.NetworkkeycardEntered;
                set => Panel.NetworkkeycardEntered = value;
            }
        }

        [System.Obsolete("Use Detonate()")]
        public void Detoante() => WarheadController.Detonate();
    }
}
