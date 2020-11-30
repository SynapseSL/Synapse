using Synapse.Api.Events.SynapseEventArguments;
using Synapse.Patches.EventsPatches.ScpPatches.Scp106;

namespace Synapse.Api.Events
{
    public class ScpEvents
    {
        internal ScpEvents() { }

        public Scp096Events Scp096 { get; } = new Scp096Events();
        
        public Scp106Events Scp106 { get; } = new Scp106Events();

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

        public class Scp106Events
        {
            internal Scp106Events() { }

            public event EventHandler.OnSynapseEvent<Scp106ContainmentEventArgs> Scp106ContainmentEvent;

            public event EventHandler.OnSynapseEvent<PocketDimensionEnterEventArgs> PocketDimensionEnterEvent;
            
            public event EventHandler.OnSynapseEvent<PocketDimensionLeaveEventArgs> PocketDimensionLeaveEvent;

            public event EventHandler.OnSynapseEvent<PortalCreateEventArgs> PortalCreateEvent;

            #region Invoke106Events

            internal void InvokeScp106ContainmentEvent(Player player, ref bool allow)
            {
                var ev = new Scp106ContainmentEventArgs {Allow = allow, Player = player};
                Scp106ContainmentEvent?.Invoke(ev);
                allow = ev.Allow;
            }

            internal void InvokePocketDimensionEnterEvent(Player player, Player scp106, ref bool allow)
            {
                var ev = new PocketDimensionEnterEventArgs
                    {Allow = allow, Scp106 = scp106, Player = player};
                PocketDimensionEnterEvent?.Invoke(ev);

                allow = ev.Allow;
            }
            
            internal void InvokePocketDimensionLeaveEvent(Player player,ref UnityEngine.Vector3 pos, ref PocketDimensionTeleport.PDTeleportType teleportType, out bool allow)
            {
                var ev = new PocketDimensionLeaveEventArgs
                {
                    ExitPosition = pos,
                    Player = player,
                    TeleportType = teleportType
                };

                PocketDimensionLeaveEvent?.Invoke(ev);

                pos = ev.ExitPosition;
                teleportType = ev.TeleportType;
                allow = ev.Allow;
            }

            internal void InvokePortalCreateEvent(Player scp106, out bool allow)
            {
                var ev = new PortalCreateEventArgs
                {
                    Scp106 = scp106,
                    Allow = true,
                };

                PortalCreateEvent?.Invoke(ev);

                allow = ev.Allow;
            }
            #endregion
        }
    }
}
