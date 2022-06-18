using UnityEngine;

namespace Synapse.Api.Events.SynapseEventArguments
{
    public class Scp096AddTargetEventArgument : ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public Player Scp096 { get; internal set; }

        public Scp096PlayerState RageState { get; internal set; }

        public bool Allow { get; set; }
    }

    public class Scp106ContainmentEventArgs : ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public bool Allow { get; set; }
    }

    public class PocketDimensionEnterEventArgs : ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public Player Scp106 { get; internal set; }

        public bool Allow { get; set; }
    }

    public class PocketDimensionLeaveEventArgs : ISynapseEventArgs
    {
        public Player Player { get; internal set; }

        public Vector3 ExitPosition { get; set; }

        public PocketDimensionTeleport.PDTeleportType TeleportType { get; set; }

        public bool Allow { get; set; } = true;
    }

    public class PortalCreateEventArgs : ISynapseEventArgs
    {
        public Player Scp106 { get; internal set; }

        public bool Allow { get; set; }
    }

    public class Scp079RecontainEventArgs : ISynapseEventArgs
    {
        public Enum.Recontain079Status Status { get; internal set; }

        public bool Allow { get; set; } = true;
    }

    public class ScpAttackEventArgs : ISynapseEventArgs
    {
        public Enum.ScpAttackType AttackType { get; internal set; }

        public Player Scp { get; internal set; }

        public Player Target { get; internal set; }

        public bool Allow { get; set; } = true;
    }

    public class Scp173PlaceTantrumEventArgs : ISynapseEventArgs
    {
        public Player Scp173 { get; internal set; }

        public bool Allow { get; set; } = true;
    }

    // I thought I could combine both but I'll let you guys do it if you want to
    public class Scp173SpeedAbilityEventArgs : ISynapseEventArgs
    {
        public Player Scp173 { get; internal set; }

        public bool Allow { get; set; } = true;
    }

    public class Scp173BlinkEventArgs : ISynapseEventArgs
    {
        public Player Scp173 { get; internal set; }

        public bool Allow { get; set; } = true;

        public Vector3 Position { get; set; }
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
    public class Scp079DoorInteractEventArgs : ISynapseEventArgs
    {
        public Player Scp079 { get; internal set; }
        public Scp079EventMisc.DoorAction Action { get; internal set; }
        public float EnergyNeeded { get; internal set; }
        public Door Door { get; internal set; }
        public Scp079EventMisc.InteractionResult Result { get; set; }
    }
    public class Scp079SpeakerInteractEventArgs : ISynapseEventArgs
    {
        public Player Scp079 { get; internal set; }
        public float EnergyNeeded { get; internal set; }
        public Scp079EventMisc.InteractionResult Result { get; set; }
    }
    public class Scp079ElevatorInteractEventArgs : ISynapseEventArgs
    {
        public Player Scp079 { get; internal set; }
        public float EnergyNeeded { get; internal set; }
        public Elevator Elevator { get; internal set; }
        public Scp079EventMisc.InteractionResult Result { get; set; }
    }
    public class Scp079RoomLockdownEventArgs : ISynapseEventArgs
    {
        public Player Scp079 { get; internal set; }
        public float EnergyNeeded { get; internal set; }
        public Room Room { get; internal set; }
        public bool LightsOut { get; set; }
        public Scp079EventMisc.InteractionResult Result { get; set; }
    }
    public class Scp079TeslaInteractEventArgs : ISynapseEventArgs
    {
        public Player Scp079 { get; internal set; }
        public float EnergyNeeded { get; internal set; }
        public Room Room { get; internal set; }
        public Tesla Tesla { get; internal set; }
        public Scp079EventMisc.InteractionResult Result { get; set; }
    }
    public class Scp079CameraSwitchEventArgs : ISynapseEventArgs
    {
        public Player Scp079 { get; internal set; }
        public Camera Camera { get; internal set; }
        public bool Spawning { get; internal set; }
        public bool MapSwitch { get; internal set; }
        public bool Allow { get; set; }
    }
    public class Scp049ReviveEvent : ISynapseEventArgs
    {
        public Player Scp049 { get; internal set; }
        public Player Target { get; internal set; }
        public Ragdoll Ragdoll { get; internal set; }
        public bool Finish { get; internal set; }
        public bool Allow { get; set; } = true;
    }
}
