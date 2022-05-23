using Synapse.Api.Events.SynapseEventArguments;
using static Synapse.Api.Events.EventHandler;

namespace Synapse.Api.Events
{
    public class SynapseObjectEvent
    {
        public event OnSynapseEvent<SOEventArgs> LoadComponentEvent;
        public event OnSynapseEvent<SOEventArgs> UpdateEvent;
        public event OnSynapseEvent<SOEventArgs> DestroyEvent;
        public event OnSynapseEvent<SOPickupEventArgs> PickupEvent;

        internal void InvokeUpdate(SOEventArgs ev) => UpdateEvent?.Invoke(ev);
        internal void InvokeLoadComponent(SOEventArgs ev) => LoadComponentEvent?.Invoke(ev);
        internal void InvokeDestroy(SOEventArgs ev) => DestroyEvent?.Invoke(ev);
        internal void InvokePickup(SOPickupEventArgs ev) => PickupEvent?.Invoke(ev);
    }
}