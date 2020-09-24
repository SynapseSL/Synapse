using Synapse.Api.Events.SynapseEventArguments;

namespace Synapse.Api.Events
{
    public class ScpEvents
    {
        internal ScpEvents() { }

        public Scp096Events Scp096 { get; } = new Scp096Events();

        public class Scp096Events
        {
            internal Scp096Events() { }

            public event EventHandler.OnSynapseEvent<Scp096AddTargetEventArgument> Scp096AddTargetEvent;

            #region InvokeEvents
            internal void InvokeScpTargetEvent(Player player, Player scp096, PlayableScps.Scp096PlayerState state, out bool allow)
            {
                allow = true;
                if (Scp096AddTargetEvent == null) return;

                var ev = new Scp096AddTargetEventArgument()
                {
                    Player = player,
                    Scp096 = scp096,
                    RageState = state,
                    Allow = true,
                };

                Scp096AddTargetEvent.Invoke(ev);

                allow = ev.Allow;
            }
            #endregion
        }
    }
}
