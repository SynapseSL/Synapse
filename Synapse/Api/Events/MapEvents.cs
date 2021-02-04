using System;
using System.Collections.Generic;
using Synapse.Api.Events.SynapseEventArguments;

namespace Synapse.Api.Events
{
    public class MapEvents
    {
        internal MapEvents() { }

        public event EventHandler.OnSynapseEvent<TriggerTeslaEventArgs> TriggerTeslaEvent;

        public event Action WarheadDetonationEvent;

        public event EventHandler.OnSynapseEvent<DoorInteractEventArgs> DoorInteractEvent;

        public event EventHandler.OnSynapseEvent<LCZDecontaminationEventArgs> LCZDecontaminationEvent;

        public event EventHandler.OnSynapseEvent<Scp914ActivateEventArgs> Scp914ActivateEvent;

        #region Invoke
        internal void InvokeTriggerTeslaEv(Player player, Tesla tesla, ref bool trigger)
        {
            trigger = true;
            var ev = new TriggerTeslaEventArgs
            {
                Player = player,
                Tesla = tesla,
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

        internal void InvokeLCZDeconEvent(out bool allow)
        {
            var ev = new LCZDecontaminationEventArgs
            {
                Allow = true,
            };

            LCZDecontaminationEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void Invoke914Activate(ref List<Player> players,ref List<Synapse.Api.Items.SynapseItem> items,out bool allow,out bool move)
        {
            var ev = new Scp914ActivateEventArgs
            {
                Items = items,
                Players = players
            };

            Scp914ActivateEvent?.Invoke(ev);

            allow = ev.Allow;
            move = ev.Move;
            players = ev.Players;
            items = ev.Items;
        }
        #endregion
    }
}
