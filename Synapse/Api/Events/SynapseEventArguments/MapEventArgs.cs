using System.Collections.Generic;

namespace Synapse.Api.Events.SynapseEventArguments
{
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

    public class LCZDecontaminationEventArgs : EventHandler.ISynapseEventArgs
    {
        public bool Allow { get; set; }
    }

    public class Scp914ActivateEventArgs : EventHandler.ISynapseEventArgs
    {
        public List<Player> Players { get; set; }

        public List<Synapse.Api.Items.SynapseItem> Items { get; set; }

        public bool Move { get; set; } = true;

        public bool Allow { get; set; } = true;
    }
}
