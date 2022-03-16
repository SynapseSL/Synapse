using Synapse.Api.CustomObjects;
using Synapse.Config;
using System.Linq;
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
            Player.PlayerRadioInteractEvent += Player_PlayerRadioInteractEvent;
#endif
        }

        private void Player_PlayerRadioInteractEvent(SynapseEventArguments.PlayerRadioInteractEventArgs ev)
        {
            Logger.Get.Debug(ev.Interaction);
            Logger.Get.Debug(ev.CurrentRange);
            ev.NextRange = InventorySystem.Items.Radio.RadioMessages.RadioRangeLevel.UltraRange;
        }

        private void KeyPress(SynapseEventArguments.PlayerKeyPressEventArgs ev)
        {
            switch (ev.KeyCode)
            {
                case KeyCode.Alpha0:
                    var locker = new SynapseLockerObject(Enum.LockerType.ScpPedestal, ev.Player.Position, Quaternion.identity, Vector3.one);

                    MEC.Timing.CallDelayed(5f,() => locker.Position = ev.Player.Position);
                    break;

                case KeyCode.Alpha1:
                    SynapseController.Server.Map.SpawnTantrum(ev.Player.Position, 10f);
                    break;

                case KeyCode.Alpha4:
                    var door = SynapseController.Server.Map.Doors.FirstOrDefault(x => x.DoorType == Enum.DoorType.LCZ_Door);
                    ev.Player.Position = door.Position;
                    var child = door.GameObject.transform.GetChild(2).GetChild(1).GetChild(0); // 0 = Left 1 = Right
                    var count = child.transform.childCount;
                    for (var i = 0; i < count; i++)
                    {
                        var childchild = child.transform.GetChild(i);
                        Logger.Get.Debug(childchild.name);
                    }

                    Logger.Get.Debug(child.transform.position);
                    door.Open = true;
                    MEC.Timing.CallDelayed(1f,() => Logger.Get.Debug(child.transform.position));
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

        public SynapseObjectEvent SynapseObject { get; } = new SynapseObjectEvent();

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

        private bool firstLoaded = false;

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
