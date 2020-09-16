using Synapse.Api.Events.SynapseEventArguments;

namespace Synapse.Api.Events
{
    public class MapEvents
    {
        internal MapEvents() { }

        public event EventHandler.OnSynapseEvent<TriggerTeslaEventArgs> TriggerTeslaEvent;

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
        #endregion
    }
}
