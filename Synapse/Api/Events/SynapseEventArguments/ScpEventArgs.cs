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
}