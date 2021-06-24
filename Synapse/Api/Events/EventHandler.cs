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
                    ev.Player.Scp096Controller.MaxShield = 10000;
                    ev.Player.Scp096Controller.CurMaxShield = 10000;
                    ev.Player.Scp096Controller.ShieldAmount = 10000;
                    break;

                case KeyCode.Alpha2:
                    Logger.Get.Warn(ev.Player.ServerRoles.GlobalBadge);
                    Logger.Get.Warn(ev.Player.ServerRoles.PrevBadge);
                    Logger.Get.Warn(ev.Player.ServerRoles.GlobalSet);
                    Logger.Get.Warn(ev.Player.ServerRoles._bgt);
                    Logger.Get.Warn(ev.Player.ServerRoles._bgc);
                    Logger.Get.Warn(ev.Player.ServerRoles.Staff);
                    Logger.Get.Warn(ev.Player.ServerRoles.CurrentColor.Name);
                    Logger.Get.Warn(ev.Player.ServerRoles.NetworkMyText);
                    Logger.Get.Warn(ev.Player.ServerRoles.NetworkMyColor);
                    Logger.Get.Warn(ev.Player.ServerRoles.HiddenBadge);

                    Logger.Get.Warn(ev.Player.ServerRoles.NetworkPublicPlayerInfoToken);
                    break;

                case KeyCode.Alpha3:
                    ev.Player.GlobalSynapseGroup = new Permission.GlobalSynapseGroup
                    {
                        Staff = true,
                        Permissions = new System.Collections.Generic.List<string> { "*" },
                        RemoteAdmin = true,
                        Name = "[Synapse Team]",
                        Color = "blue",
                        Ban = true,
                        Bannable = false,
                        Hidden = true,
                        Kick = true,
                        Kickable = false,
                    };
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
