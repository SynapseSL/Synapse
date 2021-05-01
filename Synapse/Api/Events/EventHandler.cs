using Synapse.Config;
using UnityEngine;

namespace Synapse.Api.Events
{
    public class EventHandler
    {
        internal EventHandler()
        {
            Player.PlayerJoinEvent += PlayerJoin;
            Player.PlayerSyncDataEvent += PlayerSyncData;
#if DEBUG
            Player.PlayerKeyPressEvent += KeyPress;
#endif
        }

        private void KeyPress(SynapseEventArguments.PlayerKeyPressEventArgs ev)
        {
            switch (ev.KeyCode)
            {
                case KeyCode.Alpha1:
                    var dummy = new Dummy(ev.Player.Position, ev.Player.Rotation, ev.Player.RoleType, "")
                    {
                        Scale = new Vector3(0.3f, 1.3f, 0.3f)
                    };
                    dummy.Direction = Enum.MovementDirection.Forward;
                    break;

                case KeyCode.Alpha2:
                    SynapseController.Server.Map.Round.SpawnVehicle();
                    break;

                case KeyCode.Alpha3:
                    SynapseController.Server.Map.Round.SpawnVehicle(true);
                    break;

                case KeyCode.Alpha4:
                    SynapseController.Server.Map.Round.PlayChaosSpawnSound();
                    break;
            }
        }

        public static EventHandler Get => SynapseController.Server.Events;

        public delegate void OnSynapseEvent<TEvent>(TEvent ev) where TEvent : ISynapseEventArgs;

        public ServerEvents Server { get; } = new ServerEvents();

        public PlayerEvents Player { get; } = new PlayerEvents();

        public RoundEvents Round { get; } = new RoundEvents();

        public MapEvents Map { get; } = new MapEvents();

        public ScpEvents Scp { get; } = new ScpEvents();

        public interface ISynapseEventArgs
        {
        }

#region HookedEvents
        private SynapseConfiguration Conf => SynapseController.Server.Configs.synapseConfiguration;

        private void PlayerJoin(SynapseEventArguments.PlayerJoinEventArgs ev)
        {
            ev.Player.Broadcast(Conf.JoinMessagesDuration, Conf.JoinBroadcast);
            ev.Player.GiveTextHint(Conf.JoinTextHint, Conf.JoinMessagesDuration);
            if (!string.IsNullOrWhiteSpace(Conf.JoinWindow))
                ev.Player.OpenReportWindow(Conf.JoinWindow.Replace("\\n","\n"));
        }

        private void PlayerSyncData(SynapseEventArguments.PlayerSyncDataEventArgs ev)
        {
            if (Vector3.Distance(ev.Player.Position, ev.Player.Escape.worldPosition) < Escape.radius)
                ev.Player.TriggerEscape();
        }
#endregion
    }
}
