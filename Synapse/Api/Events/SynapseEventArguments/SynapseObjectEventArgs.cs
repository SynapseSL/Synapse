using Synapse.Api.CustomObjects;
using static Synapse.Api.Events.EventHandler;

namespace Synapse.Api.Events.SynapseEventArguments
{
    public class SOEventArgs : ISynapseEventArgs
    {
        public PrimitiveSynapseObject Object { get; }

        internal SOEventArgs(PrimitiveSynapseObject so) => Object = so;
    }
}
