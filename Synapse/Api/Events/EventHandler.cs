using Synapse.Config;
using Synapse.Permission;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Synapse.Api.Events
{
    public class EventHandler
    {
        internal EventHandler()
        {
            Player.PlayerJoinEvent += PlayerJoin;
            Player.PlayerSyncDataEvent += PlayerSyncData;
            Map.DoorInteractEvent += DoorInteract;

#if DEBUG
            Player.PlayerKeyPressEvent += OnKeyPress;
#endif
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

        private void OnKeyPress(SynapseEventArguments.PlayerKeyPressEventArgs ev)
        {
            switch (ev.KeyCode)
            {
                case KeyCode.Alpha0:
                    var msg2 = "";

                    foreach (var door in Synapse.Api.Map.Get.Doors)
                        msg2 += $"\n{door.GameObject.name}";

                    foreach (var elev in Synapse.Api.Map.Get.Elevators)
                        msg2 += $"\n{elev.GameObject.name}";

                    foreach (var gen in Synapse.Api.Map.Get.Generators)
                        msg2 += $"\n{gen.GameObject.name}";

                    Logger.Get.Info(msg2);
                    break;

                case KeyCode.Alpha1:
                    foreach (var door in Synapse.Api.Map.Get.Doors)
                        door.Open = true;
                    break;

                case KeyCode.Alpha2:
                    foreach (var door in Synapse.Api.Map.Get.Doors)
                        door.Locked = true;
                    break;

                case KeyCode.Alpha3:
                    foreach (var elev in Synapse.Api.Map.Get.Elevators)
                        elev.Status = Lift.Status.Moving;
                    break;

                case KeyCode.Alpha4:
                    foreach (var elev in Synapse.Api.Map.Get.Elevators)
                        elev.Locked = true;
                    break;

                case KeyCode.Alpha5:
                    foreach (var elev in Synapse.Api.Map.Get.Elevators)
                        elev.Use();
                    break;

                case KeyCode.Alpha6:
                    foreach (var tes in Synapse.Api.Map.Get.Teslas)
                        tes.Trigger();
                    break;

                case KeyCode.Alpha7:
                    foreach (var tes in Synapse.Api.Map.Get.Teslas)
                        tes.InstantTrigger();
                    break;

                case KeyCode.Alpha8:
                    foreach (var tes in Synapse.Api.Map.Get.Teslas)
                        tes.SizeOfTrigger = tes.SizeOfTrigger * 2;
                    break;

                case KeyCode.Alpha9:
                    Logger.Get.Info($"Synapse:{ev.Player.SynapseGroup.GetVanillaPermissionValue()} SCP:{ev.Player.Rank.Permissions}");
                    break;
            }
        }

        private void PlayerJoin(SynapseEventArguments.PlayerJoinEventArgs ev)
        {
            ev.Player.Broadcast(conf.JoinMessagesDuration, conf.JoinBroadcast);
            ev.Player.Broadcast(conf.JoinMessagesDuration, conf.JoinTextHint);
        }

        private void PlayerSyncData(SynapseEventArguments.PlayerSyncDataEventArgs ev)
        {
            if (ev.Player.RoleType != RoleType.ClassD &&
                ev.Player.RoleType != RoleType.Scientist &&
                !(Vector3.Distance(ev.Player.Position, ev.Player.GetComponent<Escape>().worldPosition) >= Escape.radius))
                ev.Player.ClassManager.CmdRegisterEscape();
        }

        private void DoorInteract(SynapseEventArguments.DoorInteractEventArgs ev)
        {
            if (!SynapseController.Server.Configs.SynapseConfiguration.RemoteKeyCard) return;
            if (ev.Allow) return;

            if (!ev.Player.Items.Any()) return;
            foreach (var gameItem in ev.Player.Items.Select(item => ev.Player.Inventory.GetItemByID(item.id)).Where(gameitem => gameitem.permissions != null && gameitem.permissions.Length != 0))
            {
                ev.Allow = gameItem.permissions.Any(p =>
                    global::Door.backwardsCompatPermissions.TryGetValue(p, out var flag) &&
                    ev.Door.PermissionLevels.HasPermission(flag));
            }
        }
#endregion
    }
}
