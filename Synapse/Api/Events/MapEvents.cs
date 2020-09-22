using System;
using Synapse.Api.Events.SynapseEventArguments;

namespace Synapse.Api.Events
{
    public class MapEvents
    {
        internal MapEvents() { }

        public event EventHandler.OnSynapseEvent<TriggerTeslaEventArgs> TriggerTeslaEvent;

        public event Action WarheadDetonationEvent;

        public event EventHandler.OnSynapseEvent<DoorInteractEventArgs> DoorInteractEvent;

        #region Invoke
        internal void InvokeTriggerTeslaEv(Player player,Tesla tesla,bool hurtrange,out bool trigger)
        {
            trigger = true;
            var ev = new TriggerTeslaEventArgs
            {
                Player = player,
                Tesla = tesla,
                HurtRange = hurtrange,
                Trigger = trigger
            };

            TriggerTeslaEvent.Invoke(ev);

            trigger = ev.Trigger;
        }

        internal void InvokeDoorInteractEvent(Player player,Door door,ref bool allow)
        {
            if (DoorInteractEvent == null) return;

            var ev = new DoorInteractEventArgs
            {
                Player = player,
                Allow = allow,
                Door = door
            };

            DoorInteractEvent.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokeWarheadDetonationEvent() => WarheadDetonationEvent?.Invoke();

        #endregion
    }
}
