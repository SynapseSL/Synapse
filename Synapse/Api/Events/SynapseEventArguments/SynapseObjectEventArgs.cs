using Synapse.Api.CustomObjects;
using static Synapse.Api.Events.EventHandler;

namespace Synapse.Api.Events.SynapseEventArguments
{
    public class SOEventArgs : ISynapseEventArgs
    {
        public ISynapseObject Object { get; }

        internal SOEventArgs(ISynapseObject so) => Object = so;
    }
}
