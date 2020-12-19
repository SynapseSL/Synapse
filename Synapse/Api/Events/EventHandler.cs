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
                    ev.Player.Scp096Controller.RageState = PlayableScps.Scp096PlayerState.Attacking;
                    break;

                case KeyCode.Alpha2:
                    ev.Player.Scp096Controller.RageState = PlayableScps.Scp096PlayerState.Calming;
                    break;

                case KeyCode.Alpha3:
                    ev.Player.Scp096Controller.RageState = PlayableScps.Scp096PlayerState.Charging;
                    break;

                case KeyCode.Alpha4:
                    ev.Player.Scp096Controller.RageState = PlayableScps.Scp096PlayerState.Docile;
                    break;

                case KeyCode.Alpha5:
                    ev.Player.Scp096Controller.RageState = PlayableScps.Scp096PlayerState.Enraged;
                    break;

                case KeyCode.Alpha6:
                    ev.Player.Scp096Controller.RageState = PlayableScps.Scp096PlayerState.Enraging;
                    break;

                case KeyCode.Alpha7:
                    ev.Player.Scp096Controller.RageState = PlayableScps.Scp096PlayerState.TryNotToCry;
                    break;

                case KeyCode.Alpha8:
                    ev.Player.Scp096Controller.CurMaxShield = 100000f;
                    ev.Player.Scp096Controller.EnrageTimeLeft = 100000000f;
                    break;

                case KeyCode.Alpha9:
                    ev.Player.OpenReportWindow("<color=red>Welcome on my Server!</color>\nHere are some Rules:\n1. ...\n2. ...\n3. ...\n4. ...\n<color=blue>Thank you for Reading the Rules!</color>");
                    break;

                case KeyCode.Alpha0:
                    Logger.Get.Info(ev.Player.GlobalBadge.ToString());
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
