using Synapse.Config;
using System.Linq;
using System.IO;
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
            Player.PlayerKeyPressEvent += KeyPress;
#endif
        }

        private void KeyPress(SynapseEventArguments.PlayerKeyPressEventArgs ev)
        {
            switch (ev.KeyCode)
            {
                case KeyCode.Alpha1:
                    ev.Player.WeaponManager.RpcEmptyClip();
                    break;

                case KeyCode.Alpha2:
                    Logger.Get.Info(SynapseController.Server.FF.ToString());
                    break;

                case KeyCode.Alpha3:
                    SynapseController.Server.Map.HeavyController.Overcharge(true);
                    break;

                case KeyCode.Alpha4:
                    SynapseController.Server.Map.HeavyController.Overcharge(false);
                    break;

                case KeyCode.Alpha5:
                    SynapseController.Server.Map.Decontamination.Locked = true;
                    break;

                case KeyCode.Alpha6:
                    SynapseController.Server.Map.Decontamination.Locked = false;
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

            if (!ev.Player.VanillaItems.Any()) return;
            foreach (var gameItem in ev.Player.VanillaItems.Select(item => ev.Player.VanillaInventory.GetItemByID(item.id)).Where(gameitem => gameitem.permissions != null && gameitem.permissions.Length != 0))
            {
                if (gameItem.permissions.Any(p =>
                     global::Door.backwardsCompatPermissions.TryGetValue(p, out var flag) &&
                     ev.Door.PermissionLevels.HasPermission(flag)))
                    ev.Allow = true;
            }
        }
#endregion
    }
}
