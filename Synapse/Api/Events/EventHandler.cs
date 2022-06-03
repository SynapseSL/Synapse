using Synapse.Api.CustomObjects;
using Synapse.Api.CustomObjects.CustomRooms;
using Synapse.Config;
using Synapse.Api.Events.SynapseEventArguments;
using UnityEngine;

namespace Synapse.Api.Events
{
    public interface ISynapseEventArgs { }
    public delegate void OnSynapseEvent<TEvent>(TEvent ev) where TEvent : ISynapseEventArgs;

    public class EventHandler
    {
        public static EventHandler Get
            => SynapseController.Server.Events;

        public ServerEvents Server { get; }
        public PlayerEvents Player { get; }
        public RoundEvents Round { get; }
        public MapEvents Map { get; }
        public ScpEvents Scp { get; }
        public SynapseObjectEvent SynapseObject { get; }

        private SynapseConfiguration Conf
            => SynapseController.Server.Configs.SynapseConfiguration;

        private SerializedPlayerState state;
        private bool firstLoaded = false;

        internal EventHandler()
        {
            Server = new ServerEvents();
            Player = new PlayerEvents();
            Round = new RoundEvents();
            Map = new MapEvents();
            Scp = new ScpEvents();
            SynapseObject = new SynapseObjectEvent();

            Player.PlayerJoinEvent += PlayerJoin;
            Round.RoundRestartEvent += RounRestart;
            Round.WaitingForPlayersEvent += Waiting;
            Player.LoadComponentsEvent += LoadPlayer;
            Server.UpdateEvent += OnUpdate;
#if DEBUG
            Player.PlayerKeyPressEvent += KeyPress;
#endif
        }

        private void KeyPress(SynapseEventArguments.PlayerKeyPressEventArgs ev)
        {
            switch (ev.KeyCode)
            {
                case KeyCode.Alpha1:
                    CustomRoomHandler.Get.SpawnCustomRoom(0, ev.Player.Position);
                    break;
            }
        }

        private void LoadPlayer(LoadComponentEventArgs ev)
        {
            if (ev.Player.GetComponent<Player>() is null)
                _ = ev.Player.gameObject.AddComponent<Player>();
        }

        private void Waiting()
        {
            SynapseController.Server.Map.AddObjects();
            SynapseController.Server.Map.Round.CurrentRound++;

            if (!firstLoaded)
            {
                firstLoaded = true;
                SynapseController.CommandHandlers.GenerateCommandCompletion();

                SchematicHandler.Get.InitLate();
            }
        }

        private void RounRestart()
            => Api.Map.Get.ClearObjects();

        private void PlayerJoin(PlayerJoinEventArgs ev)
        {
            ev.Player.Broadcast(Conf.JoinMessagesDuration, Conf.JoinBroadcast);
            ev.Player.GiveTextHint(Conf.JoinTextHint, Conf.JoinMessagesDuration);
            if (!System.String.IsNullOrWhiteSpace(Conf.JoinWindow))
                ev.Player.OpenReportWindow(Conf.JoinWindow.Replace("\\n", "\n"));
        }

        private void OnUpdate()
        {
            foreach (var player in SynapseController.Server.Players)
            {
                if (Vector3.Distance(player.Position, player.Escape.worldPosition) < Escape.radius)
                    player.TriggerEscape();

                Player.InvokePlayerSyncDataEvent(player, out _);
            }
        }
    }
}
