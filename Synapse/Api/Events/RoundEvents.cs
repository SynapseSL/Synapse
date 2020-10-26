using System;
using System.Collections.Generic;
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

        public event EventHandler.OnSynapseEvent<SpawnPlayersEventArgs> SpawnPlayersEvent;

        #region PlayerEventsInvoke
        internal void InvokeWaitingForPlayers() => WaitingForPlayersEvent?.Invoke();
        internal void InvokeRoundStartEvent() => RoundStartEvent?.Invoke();
        internal void InvokeRoundRestartEvent() => RoundRestartEvent?.Invoke();
        internal void InvokeRoundEndEvent() => RoundEndEvent?.Invoke();


        internal void InvokeRoundCheckEvent(ref bool allow,ref RoundSummary.LeadingTeam leadingTeam)
        {
            var ev = new RoundCheckEventArgs
            {
                Team = leadingTeam,
                EndRound = allow,
            };

            RoundCheckEvent?.Invoke(ev);

            allow = ev.EndRound;
            leadingTeam = ev.Team;
        }

        internal void InvokeSpawnPlayersEvent(ref Dictionary<Player, int> spawnplayers, out bool allow)
        {
            var ev = new SpawnPlayersEventArgs
            {
                SpawnPlayers = spawnplayers,
                Allow = true,
            };

            SpawnPlayersEvent?.Invoke(ev);

            allow = ev.Allow;
        }
        #endregion
    }
}
