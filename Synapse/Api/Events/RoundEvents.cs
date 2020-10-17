using System;
using Synapse.Api.Events.SynapseEventArguments;

namespace Synapse.Api.Events
{
    public class RoundEvents
    {
        internal RoundEvents() { }

        public event Action WaitingForPlayersEvent;
        
        public event Action RoundStartEvent;

        public event Action RoundRestartEvent;

        public event Action RoundEndEvent;

        public event EventHandler.OnSynapseEvent<RoundCheckEventArgs> RoundCheckEvent; 

        #region PlayerEventsInvoke
        internal void InvokeWaitingForPlayers() => WaitingForPlayersEvent?.Invoke();
        internal void InvokeRoundStartEvent() => RoundStartEvent?.Invoke();
        internal void InvokeRoundRestartEvent() => RoundRestartEvent?.Invoke();
        internal void InvokeRoundEndEvent() => RoundEndEvent?.Invoke();


        internal void InvokeRoundCheckEvent(ref bool forceEnd, ref bool allow, ref RoundSummary.LeadingTeam team,
            ref bool teamChanged)
        {
            var ev = new RoundCheckEventArgs
                {Allow = allow, Team = team, ForceEnd = forceEnd, TeamChanged = teamChanged};
            RoundCheckEvent?.Invoke(ev);

            allow = ev.Allow;
            team = ev.Team;
            forceEnd = ev.ForceEnd;
            teamChanged = ev.TeamChanged;
        }

        #endregion
    }
}
