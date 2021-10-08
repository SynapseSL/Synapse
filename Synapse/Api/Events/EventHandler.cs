using Synapse.Config;
using UnityEngine;

namespace Synapse.Api.Events
{
    public class EventHandler
    {
        internal EventHandler()
        {
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
                    foreach(var item in Synapse.Api.Items.SynapseItem.AllItems)
                    {
                        if (item.Value == null) Logger.Get.Warn(item.Key + " - null");
                        else Logger.Get.Warn($"{item.Key} - {item.Value.ItemType}");
                    }
                    break;
                case KeyCode.Alpha2:
                    Logger.Get.Warn(ev.Player.ItemInHand.Serial);
                    ev.Player.ItemInHand.Scale = Vector3.one * 3;
                    break;
                case KeyCode.Alpha3:
                    foreach (var item in ev.Player.VanillaInventory.UserInventory.Items)
                    {
                        Logger.Get.Warn($"Null: {item.Value == null}");
                        Logger.Get.Warn($"Serial: {item.Key}");
                    }
                    break;
                case KeyCode.Alpha4:
                    new WorkStation(ev.Player.Position, ev.Player.transform.rotation.eulerAngles, Vector3.one);
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

        private void LoadPlayer(SynapseEventArguments.LoadComponentEventArgs ev)
        {
            if (ev.Player.GetComponent<Player>() == null)
                ev.Player.gameObject.AddComponent<Player>();
        }

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

        private void OnUpdate()
        {
            foreach (var player in SynapseController.Server.Players)
            {
                if (Vector3.Distance(player.Position, player.Escape.worldPosition) < Escape.radius)
                    player.TriggerEscape();

                Player.InvokePlayerSyncDataEvent(player, out _);
            }
        }
        #endregion
    }
}
