using Synapse.Api.Items;
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
            Round.RoundRestartEvent += RounRestart;
            Round.WaitingForPlayersEvent += Waiting;
#if DEBUG
            Player.PlayerKeyPressEvent += KeyPress;
#endif
        }

        private void KeyPress(SynapseEventArguments.PlayerKeyPressEventArgs ev)
        {
            switch (ev.KeyCode)
            {
                case KeyCode.Alpha1:
                    ev.Player.Scp096Controller.MaxShield = 10000;
                    ev.Player.Scp096Controller.CurMaxShield = 10000;
                    ev.Player.Scp096Controller.ShieldAmount = 10000;
                    SerializedMapPoint point = ev.Player.MapPoint;
                    ev.Player.MapPoint = (MapPoint)point;
                    SerializedItem item = ev.Player.ItemInHand;
                    ev.Player.ItemInHand = (Items.SynapseItem)item;
                    ev.Player.Position = new SerializedVector3(ev.Player.Position);
                    break;

                case KeyCode.Alpha2:
                    Logger.Get.Warn("Alpha2");
                    foreach (var item2 in SynapseItem.AllItems.Values)
                        if (item2 != null)
                            Logger.Get.Warn($"{item2.Serial} {item2.ItemType}");
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

        private void Waiting()
        {
            SynapseController.Server.Map.AddObjects();
            SynapseController.Server.Map.Round.CurrentRound++;
        }

        private void RounRestart() => Synapse.Api.Map.Get.ClearObjects();

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
