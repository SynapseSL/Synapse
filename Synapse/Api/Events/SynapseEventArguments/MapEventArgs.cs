﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.Events.SynapseEventArguments
{
    public class GeneratorServerSetFlagEventArgs : EventHandler.ISynapseEventArgs
    {
        public Generator Generator { get; internal set; }

        public MapGeneration.Distributors.Scp079Generator.GeneratorFlags Flag { get; internal set; }

        public bool State { get; internal set; }

        public bool Allow { get; set; }
    }

    public class TriggerTeslaEventArgs : EventHandler.ISynapseEventArgs
    {
        public Tesla Tesla { get; internal set; }

        public Player Player { get; internal set; }

        public bool Trigger { get; set; }
    }

    public class DoorInteractEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public Door Door { get; internal set; }

        public bool Allow { get; set; }
    }

    public class LockerInteractEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }
        public LockerChamber LockerChamber { get; internal set; }
        public bool Allow { get; set; }
    }

    public class LCZDecontaminationEventArgs : EventHandler.ISynapseEventArgs
    {
        public bool Allow { get; set; }
    }

    public class Scp914ActivateEventArgs : EventHandler.ISynapseEventArgs
    {
        public List<Player> Players { get; set; }

        public List<Synapse.Api.Items.SynapseItem> Items { get; set; }

        public Vector3 MoveVector { get; set; }

        public bool Allow { get; set; } = true;

        [Obsolete("Use MoveVector instead and set it to Vector3.zero")]
        public bool Move { get; set; } = true;
    }

    public class LiftMoveObjectsEventArgs : EventHandler.ISynapseEventArgs
    {
        public Elevator Elevator { get; internal set; }

        internal Transform Transform { get; set; }
        internal bool DeleteTransform { get; set; } = false;
        public Vector3 Position
        {
            get => Transform.position;
            set
            {
                DeleteTransform = true;
                var obj = new GameObject("LiftPosition");
                obj.transform.position = value;
                Transform = obj.transform;
            }
        }

        public bool Allow { get; set; } = true;
    }
}
