using System;

namespace Synapse.Api.Events
{
    public class RoundEvents
    {
        internal RoundEvents() { }

        public event Action WaitingForPlayersEvent;

        #region PlayerEventsInvoke
        internal void InvokeWaitingForPlayers() => WaitingForPlayersEvent?.Invoke();
        #endregion
    }
}
