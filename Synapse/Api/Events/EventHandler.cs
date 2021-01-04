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
                    Logger.Get.Info(ev.Player.NicknameSync.Network_playerInfoToShow.ToString());
                    ev.Player.DisplayInfo = "TestRole";
                    ev.Player.NicknameSync.Network_playerInfoToShow &= ~PlayerInfoArea.Role;
                    Logger.Get.Info(ev.Player.NicknameSync.Network_playerInfoToShow.ToString());
                    break;

                case KeyCode.Alpha2:
                    ev.Player.DisplayInfo = "TestRole";
                    break;

                case KeyCode.Alpha3:
                    ev.Player.RemoveDisplayInfo(PlayerInfoArea.Role);
                    break;

                case KeyCode.Alpha4:
                    ev.Player.AddDisplayInfo(PlayerInfoArea.Role);
                    break;

                case KeyCode.Alpha5:
                    ev.Player.RemoveDisplayInfo(PlayerInfoArea.Nickname);
                    break;

                case KeyCode.Alpha6:
                    ev.Player.AddDisplayInfo(PlayerInfoArea.Nickname);
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
        private SynapseConfiguration conf => SynapseController.Server.Configs.SynapseConfiguration;

        private void PlayerJoin(SynapseEventArguments.PlayerJoinEventArgs ev)
        {
            ev.Player.Broadcast(conf.JoinMessagesDuration, conf.JoinBroadcast);
            ev.Player.GiveTextHint(conf.JoinTextHint, conf.JoinMessagesDuration);
            if (!string.IsNullOrWhiteSpace(conf.JoinWindow))
                ev.Player.OpenReportWindow(conf.JoinWindow.Replace("\\n","\n"));
        }

        private void PlayerSyncData(SynapseEventArguments.PlayerSyncDataEventArgs ev)
        {
            if (ev.Player.RoleType != RoleType.ClassD &&
                ev.Player.RoleType != RoleType.Scientist &&
                !(Vector3.Distance(ev.Player.Position, ev.Player.GetComponent<Escape>().worldPosition) >= Escape.radius))
                ev.Player.ClassManager.CmdRegisterEscape();
        }
#endregion
    }
}
