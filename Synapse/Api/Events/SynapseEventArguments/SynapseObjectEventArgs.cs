using Synapse.Api.CustomObjects;

namespace Synapse.Api.Events.SynapseEventArguments
{
    public class SOEventArgs : ISynapseEventArgs
    {
        public ISynapseObject Object { get; }

        internal SOEventArgs(ISynapseObject so)
            => Object = so;
    }

    public class SOPickupEventArgs : ISynapseEventArgs
    {
        public SynapseItemObject Object { get; internal set; }

        public Player Player { get; internal set; }

        public bool Allow { get; set; } = true;
    }
}
