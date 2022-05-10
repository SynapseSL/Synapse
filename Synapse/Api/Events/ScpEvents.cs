using Synapse.Api.Enum;
using Synapse.Api.Events.SynapseEventArguments;
using UnityEngine;

namespace Synapse.Api.Events
{
    public class ScpEvents
    {
        internal ScpEvents() { }

        public Scp096Events Scp096 { get; } = new Scp096Events();

        public Scp106Events Scp106 { get; } = new Scp106Events();

        public Scp079Events Scp079 { get; } = new Scp079Events();

        public Scp173Events Scp173 { get; } = new Scp173Events();

        public Scp049Events Scp049 { get; } = new Scp049Events();

        public event EventHandler.OnSynapseEvent<ScpAttackEventArgs> ScpAttackEvent;

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
                var ev = new Scp106ContainmentEventArgs { Allow = allow, Player = player };
                Scp106ContainmentEvent?.Invoke(ev);
                allow = ev.Allow;
            }

            internal void InvokePocketDimensionEnterEvent(Player player, Player scp106, ref bool allow)
            {
                var ev = new PocketDimensionEnterEventArgs
                { Allow = allow, Scp106 = scp106, Player = player };
                PocketDimensionEnterEvent?.Invoke(ev);

                allow = ev.Allow;
            }

            internal void InvokePocketDimensionLeaveEvent(Player player, ref UnityEngine.Vector3 pos, ref PocketDimensionTeleport.PDTeleportType teleportType, out bool allow)
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

        public class Scp079Events
        {
            internal Scp079Events() { }

            public event EventHandler.OnSynapseEvent<Scp079RecontainEventArgs> RecontainEvent;

            public event EventHandler.OnSynapseEvent<Scp079DoorInteractEventArgs> DoorInteract;

            public event EventHandler.OnSynapseEvent<Scp079SpeakerInteractEventArgs> SpeakerInteract;

            public event EventHandler.OnSynapseEvent<Scp079ElevatorInteractEventArgs> ElevatorInteract;

            public event EventHandler.OnSynapseEvent<Scp079RoomLockdownEventArgs> RoomLockdown;

            public event EventHandler.OnSynapseEvent<Scp079TeslaInteractEventArgs> TeslaInteract;

            public event EventHandler.OnSynapseEvent<Scp079CameraSwitchEventArgs> CameraSwitch;

            internal void Invoke079RecontainEvent(Recontain079Status status, out bool allow)
            {
                var ev = new Scp079RecontainEventArgs
                {
                    Status = status
                };

                RecontainEvent?.Invoke(ev);

                allow = ev.Allow;
            }

            internal void Invoke079DoorInteract(
                Player player,
                Scp079EventMisc.DoorAction action,
                Scp079EventMisc.InteractionResult intendedResult,
                float energyNeeded,
                Door door,
                out Scp079EventMisc.InteractionResult actualResult
                )
            {
                var ev = new Scp079DoorInteractEventArgs
                {
                    Scp079 = player,
                    Action = action,
                    EnergyNeeded = energyNeeded,
                    Result = intendedResult,
                    Door = door
                };

                DoorInteract?.Invoke(ev);

                actualResult = ev.Result;
            }

            internal void Invoke079SpeakerInteract(
                Player player,
                float energyNeeded,
                Scp079EventMisc.InteractionResult intendedResult,
                out Scp079EventMisc.InteractionResult actualResult
                )
            {
                var ev = new Scp079SpeakerInteractEventArgs
                {
                    Scp079 = player,
                    Result = intendedResult,
                    EnergyNeeded = energyNeeded
                };

                SpeakerInteract?.Invoke(ev);

                actualResult = ev.Result;
            }

            internal void Invoke079ElevatorUse(
                Player player,
                float energyNeeded,
                Elevator elevator,
                Scp079EventMisc.InteractionResult intendedResult,
                out Scp079EventMisc.InteractionResult actualResult
                )
            {
                var ev = new Scp079ElevatorInteractEventArgs
                {
                    Scp079 = player,
                    Result = intendedResult,
                    EnergyNeeded = energyNeeded,
                    Elevator = elevator
                };

                ElevatorInteract?.Invoke(ev);

                actualResult = ev.Result;
            }

            internal void Invoke079RoomLockdown(
                Player player,
                float energyNeeded,
                Room room,
                ref bool lightsOut,
                Scp079EventMisc.InteractionResult intendedResult,
                out Scp079EventMisc.InteractionResult actualResult
                )
            {
                var ev = new Scp079RoomLockdownEventArgs
                {
                    Scp079 = player,
                    Result = intendedResult,
                    EnergyNeeded = energyNeeded,
                    Room = room,
                    LightsOut = lightsOut
                };

                RoomLockdown?.Invoke(ev);

                actualResult = ev.Result;
            }

            internal void Invoke079TeslaInteract(
                Player player,
                float energyNeeded,
                Room room,
                Tesla tesla,
                Scp079EventMisc.InteractionResult intendedResult,
                out Scp079EventMisc.InteractionResult actualResult
                )
            {
                var ev = new Scp079TeslaInteractEventArgs
                {
                    Scp079 = player,
                    Result = intendedResult,
                    EnergyNeeded = energyNeeded,
                    Room = room,
                    Tesla = tesla
                };

                TeslaInteract?.Invoke(ev);

                actualResult = ev.Result;
            }

            internal void Invoke079CameraSwitch(
                Player player,
                Camera cam,
                bool mapSwitch,
                bool spawning,
                out bool allow
                )
            {
                var ev = new Scp079CameraSwitchEventArgs
                {
                    Scp079 = player,
                    MapSwitch = mapSwitch,
                    Camera = cam,
                    Spawning = spawning,
                    Allow = true
                };

                CameraSwitch?.Invoke(ev);

                //If allowed by the event methods, or if this is the first spawning
                allow = ev.Allow || ev.Spawning;
            }
        }

        public class Scp173Events
        {
            internal Scp173Events() { }

            public event EventHandler.OnSynapseEvent<Scp173BlinkEventArgs> Scp173BlinkEvent;

            public event EventHandler.OnSynapseEvent<Scp173PlaceTantrumEventArgs> Scp173PlaceTantrum;

            internal void InvokeScp173PlaceTantrumEvent(Player scp173, out bool allow)
            {
                var ev = new Scp173PlaceTantrumEventArgs()
                {
                    Scp173 = scp173
                };

                Scp173PlaceTantrum?.Invoke(ev);
                
                allow = ev.Allow;
            }

            internal void InvokeScp173BlinkEvent(Player scp173, ref Vector3 pos, out bool allow)
            {
                var ev = new Scp173BlinkEventArgs
                {
                    Scp173 = scp173,
                    Position = pos
                };

                Scp173BlinkEvent?.Invoke(ev);

                allow = ev.Allow;
                pos = ev.Position;
            }
        }

        public class Scp049Events
        {
            internal Scp049Events() { }

            public event EventHandler.OnSynapseEvent<Scp049ReviveEvent> Scp049ReviveEvent;

            internal void InvokeRevive(Player scp, Player target, Ragdoll rag,bool finish,out bool allow)
            {
                var ev = new Scp049ReviveEvent
                {
                    Ragdoll = rag,
                    Scp049 = scp,
                    Target = target,
                    Finish = finish,
                };

                Scp049ReviveEvent?.Invoke(ev);

                allow = ev.Allow;
            }
        }

        internal void InvokeScpAttack(Player scp, Player target, Enum.ScpAttackType attackType, out bool allow)
        {
            var ev = new ScpAttackEventArgs
            {
                Scp = scp,
                Target = target,
                AttackType = attackType,
            };

            ScpAttackEvent?.Invoke(ev);

            allow = ev.Allow;
        }
    }
}
