using Synapse.Api.CustomObjects;
using static Synapse.Api.Events.EventHandler;

namespace Synapse.Api.Events.SynapseEventArguments
{
    public class SOEventArgs : ISynapseEventArgs
    {
        public SynapseObject Object { get; }

        internal SOEventArgs(SynapseObject so) => Object = so;
    }
}
