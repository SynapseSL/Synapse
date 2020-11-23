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
                    ev.Player.Jail.JailPlayer(ev.Player);
                    break;

                case KeyCode.Alpha2:
                    ev.Player.Jail.UnJailPlayer();
                    break;

                case KeyCode.Alpha3:
                    var msg = $"All Gameobject on MapSeed: {SynapseController.Server.Map.Seed}";

                    foreach (var go in SynapseController.Server.GetObjectsOf<GameObject>())
                        msg += $"\n{go.name} - Position: {go.transform.position}";

                    var path = Path.Combine(SynapseController.Server.Files.SynapseDirectory, "Gameobjects.txt");

                    if (!File.Exists(path))
                        File.Create(path).Close();

                    File.WriteAllText(path,msg);
                    break;

                case KeyCode.Alpha4:
                    msg = "All Rooms:";

                    foreach (var room in SynapseController.Server.Map.Rooms)
                        msg += $"\nName:{room.RoomName} Zone:{room.Zone} Type:{room.RoomType}";

                    ev.Player.SendConsoleMessage(msg);
                    break;

                case KeyCode.Alpha5:
                    SynapseController.Server.Map.SpawnGrenade(ev.Player.Position, Vector3.zero, 10f);
                    SynapseController.Server.Map.SpawnGrenade(ev.Player.Position, Vector3.zero, 10f,Enum.GrenadeType.Flashbang);
                    SynapseController.Server.Map.SpawnGrenade(ev.Player.Position, Vector3.zero, 10f,Enum.GrenadeType.Scp018);
                    break;

                case KeyCode.Alpha6:
                    SynapseController.Server.Map.SpawnGrenade(ev.Player.Position, Vector3.zero, 10f,Enum.GrenadeType.Grenade ,ev.Player);
                    SynapseController.Server.Map.SpawnGrenade(ev.Player.Position, Vector3.zero, 10f, Enum.GrenadeType.Flashbang, ev.Player);
                    SynapseController.Server.Map.SpawnGrenade(ev.Player.Position, Vector3.zero, 10f, Enum.GrenadeType.Scp018, ev.Player);
                    break;

                case KeyCode.Alpha7:
                    SynapseController.Server.Map.Explode(ev.Player.Position);
                    SynapseController.Server.Map.Explode(ev.Player.Position, Enum.GrenadeType.Flashbang);
                    SynapseController.Server.Map.SpawnGrenade(ev.Player.Position, Vector3.zero, 10f, Enum.GrenadeType.Scp018);
                    break;

                case KeyCode.Alpha8:
                    msg = $"All Doors on MapSeed: {SynapseController.Server.Map.Seed}";

                    foreach (var door in SynapseController.Server.Map.Doors)
                        msg += $"\nName:{door.Name} Type:{door.DoorType}";

                    path = Path.Combine(SynapseController.Server.Files.SynapseDirectory, "Gameobjects.txt");

                    if (!File.Exists(path))
                        File.Create(path).Close();

                    File.WriteAllText(path, msg);
                    break;

                case KeyCode.Alpha9:
                    foreach (var recipe in SynapseController.Server.Map.Scp914.Recipes)
                    {
                        msg = $"All Recipes for {recipe.itemID}";

                        msg += "\nRough:";
                        foreach (var id in recipe.rough)
                        {
                            msg += $"\n- {id}";
                        }
                        msg += "\nCoarse:";
                        foreach (var id in recipe.coarse)
                        {
                            msg += $"\n- {id}";
                        }
                        msg += "\n1:1:";
                        foreach (var id in recipe.oneToOne)
                        {
                            msg += $"\n- {id}";
                        }
                        msg += "\nfine:";
                        foreach (var id in recipe.fine)
                        {
                            msg += $"\n- {id}";
                        }
                        msg += "\nVeryFine:";
                        foreach (var id in recipe.veryFine)
                        {
                            msg += $"\n- {id}";
                        }

                        Logger.Get.Info(msg);
                    }
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
