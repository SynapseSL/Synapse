using Synapse.Api.Events.SynapseEventArguments;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.Events
{
    public class MapEvents
    {
        internal MapEvents() { }

        public event EventHandler.OnSynapseEvent<TriggerTeslaEventArgs> TriggerTeslaEvent;

        public event EventHandler.OnSynapseEvent<GeneratorEngageEventArgs> GeneratorEngageEvent;

        public event Action WarheadDetonationEvent;

        public event EventHandler.OnSynapseEvent<WarheadDetonationCanceledEventArgs> WarheadDetonationCanceledEvent;

        public event EventHandler.OnSynapseEvent<DoorInteractEventArgs> DoorInteractEvent;

        public event EventHandler.OnSynapseEvent<LCZDecontaminationEventArgs> LCZDecontaminationEvent;

        public event EventHandler.OnSynapseEvent<Scp914ActivateEventArgs> Scp914ActivateEvent;

        public event EventHandler.OnSynapseEvent<LockerInteractEventArgs> LockerInteractEvent;

        public event EventHandler.OnSynapseEvent<LiftMoveObjectsEventArgs> LiftMoveObjectsEvent;

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

            TriggerTeslaEvent?.Invoke(ev);

            trigger = ev.Trigger;
        }

        internal void InvokeGeneratorEngage(MapGeneration.Distributors.Scp079Generator generator, ref bool allow)
        {
            GeneratorEngageEventArgs ev = new GeneratorEngageEventArgs()
            {
                Generator = generator.GetGenerator(),
                Allow = allow,
            };

            GeneratorEngageEvent?.Invoke(ev);
            allow = ev.Allow;
        }

        internal void InvokeDoorInteractEvent(Player player, Door door, ref bool allow)
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

        internal void InvokeLockerInteractEvent(Player player, LockerChamber locker, ref bool allow)
        {
            var ev = new LockerInteractEventArgs
            {
                Player = player,
                Allow = allow,
                LockerChamber = locker
            };

            LockerInteractEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void InvokeWarheadDetonationEvent() => WarheadDetonationEvent?.Invoke();

        internal void InvokeWarheadDetonationCanceledEvent(out bool allow, ref GameObject disabler)
        {
            var ev = new WarheadDetonationCanceledEventArgs
            {
                Disabler = disabler,
                Allow = true,
            };

            WarheadDetonationCanceledEvent?.Invoke(ev);

            disabler = ev.Disabler;
            allow = ev.Allow;
        }

        internal void InvokeLCZDeconEvent(out bool allow)
        {
            var ev = new LCZDecontaminationEventArgs
            {
                Allow = true,
            };

            LCZDecontaminationEvent?.Invoke(ev);

            allow = ev.Allow;
        }

        internal void Invoke914Activate(ref List<Player> players, ref List<Synapse.Api.Items.SynapseItem> items, ref Vector3 moveVector, out bool allow)
        {
            var ev = new Scp914ActivateEventArgs
            {
                Items = items,
                Players = players,
                MoveVector = moveVector
            };

            Scp914ActivateEvent?.Invoke(ev);

            allow = ev.Allow;
            players = ev.Players;
            items = ev.Items;
            moveVector = ev.MoveVector;
        }

        internal void InvokeLiftMoveObjects(LiftMoveObjectsEventArgs ev) => LiftMoveObjectsEvent?.Invoke(ev);
        #endregion
    }
}