using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.Events.SynapseEventArguments
{
    public class Scp096AddTargetEventArgument : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public Player Scp096 { get; internal set; }

        public PlayableScps.Scp096PlayerState RageState { get; internal set; }

        public bool Allow { get; set; }
    }

    public class Scp106ContainmentEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public bool Allow { get; set; }
    }

    public class PocketDimensionEnterEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public Player Scp106 { get; internal set; }

        public bool Allow { get; set; }
    }

    public class PocketDimensionLeaveEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public Vector3 ExitPosition { get; set; }

        public PocketDimensionTeleport.PDTeleportType TeleportType { get; set; }

        public bool Allow { get; set; } = true;
    }

    public class PortalCreateEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Scp106 { get; internal set; }

        public bool Allow { get; set; }
    }

    public class Scp079RecontainEventArgs : EventHandler.ISynapseEventArgs
    {
        public Enum.Recontain079Status Status { get; internal set; }

        public bool Allow { get; set; } = true;
    }

    public class ScpAttackEventArgs : EventHandler.ISynapseEventArgs
    {
        public Enum.ScpAttackType AttackType { get; internal set; }

        public Player Scp { get; internal set; }

        public Player Target { get; internal set; }

        public bool Allow { get; set; } = true;
    }

    public class Scp173BlinkEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Scp173 { get; internal set; }
    }


    public class Scp079EventMisc
    {
        public enum InteractionResult
        {
            Allow,
            Disallow,
            NoEnergy
        }
        public enum DoorAction
        {
            Opening,
            Closing,
            Locking,
            Unlocking
        }
    }
    public class Scp079DoorInteractEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Scp079 { get; internal set; }
        public Scp079EventMisc.DoorAction Action { get; internal set; }
        public float EnergyNeeded { get; internal set; }
        public Door Door { get; internal set; }
        public Scp079EventMisc.InteractionResult Result { get; set; }

    }
    public class Scp079SpeakerInteractEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Scp079 { get; internal set; }
        public float EnergyNeeded { get; internal set; }
        public Scp079EventMisc.InteractionResult Result { get; set; }
    }
    public class Scp079ElevatorUseEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Scp079 { get; internal set; }
        public float EnergyNeeded { get; internal set; }
        public Elevator Elevator { get; internal set; }
        public Scp079EventMisc.InteractionResult Result { get; set; }
    }
    public class Scp079RoomLockdownEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Scp079 { get; internal set; }
        public float EnergyNeeded { get; internal set; }
        public Room Room { get; internal set; }
        public bool LightsOut { get; set; }
        public Scp079EventMisc.InteractionResult Result { get; set; }
    }
    public class Scp079TeslaInteractEventArgs : EventHandler.ISynapseEventArgs
    {
        public Player Scp079 { get; internal set; }
        public float EnergyNeeded { get; internal set; }
        public Room Room { get; internal set; }
        public Tesla Tesla { get; internal set; }
        public Scp079EventMisc.InteractionResult Result { get; set; }
    }
}