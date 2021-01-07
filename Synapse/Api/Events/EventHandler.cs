using Synapse.Config;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

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
                    foreach (var player in SynapseController.Server.Players)
                        if (player != ev.Player)
                            ev.Player.Scp173Controller.IgnoredPlayers.Add(player);
                    break;

                case KeyCode.Alpha2:
                    var msg = "";
                    foreach (var door in Api.Map.Get.Doors)
                        msg += $"Type: {door.DoorType} Name: {door}\n";

                    var path = Path.Combine(SynapseController.Server.Files.SynapseDirectory, "doors.txt");
                    if (!File.Exists(path))
                        File.Create(path).Close();
                    File.WriteAllText(path, msg);
                    break;

                case KeyCode.Alpha3:
                    ev.Player.Position = Api.Map.Get.GetDoor(Enum.DoorType.Other).Position;
                    break;

                case KeyCode.Alpha4:
                    ev.Player.Position = Api.Map.Get.GetDoor(Enum.DoorType.Airlock).Position;
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
