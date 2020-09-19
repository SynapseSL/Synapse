﻿using Dissonance;
using UnityEngine;

namespace Synapse.Api
{
    public class Nuke
    {
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

        public void Detoante() => WarheadController.Detonate();

        public void InstantPrepare() => WarheadController.InstantPrepare();

        public void Shake() => WarheadController.RpcShake(false);

        public class NukeInsidePanel
        {
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
            internal NukeOutsidePanel() { Panel = Server.Get.Host.GetComponent<AlphaWarheadOutsitePanel>(); }

            private readonly AlphaWarheadOutsitePanel Panel;

            public bool KeyCardEntered
            {
                get => Panel.NetworkkeycardEntered;
                set => Panel.NetworkkeycardEntered = value;
            }
        }
    }
}
