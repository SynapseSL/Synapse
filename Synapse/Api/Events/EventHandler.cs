using System;
using MEC;
using Synapse.Config;
using System.IO;
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
                case KeyCode.Alpha1:
                    foreach (var gen in SynapseController.Server.Map.Generators)
                        gen.ConnectedTabled = new Items.SynapseItem(ItemType.Medkit, 0, 0, 0, 0);
                    break;

                case KeyCode.Alpha2:
                    foreach (var gen in SynapseController.Server.Map.Generators)
                        gen.Open = true;
                    break;

                case KeyCode.Alpha3:
                    foreach (var gen in SynapseController.Server.Map.Generators)
                        gen.Open = false;
                    break;

                case KeyCode.Alpha4:
                    foreach (var gen in SynapseController.Server.Map.Generators)
                        gen.Locked = false;
                    break;

                case KeyCode.Alpha5:
                    foreach (var gen in SynapseController.Server.Map.Generators)
                        gen.Locked = true;
                    break;

                case KeyCode.Alpha6:
                    foreach (var gen in SynapseController.Server.Map.Generators)
                        if (gen.Room != null)
                            Logger.Get.Info(gen.Room.RoomName);
                    break;

                case KeyCode.Alpha8:
                    int delay = 1;
                    foreach (var gen in SynapseController.Server.Map.Generators)
                    {
                        Timing.CallDelayed(delay, () => gen.ConnectedTabled = new Items.SynapseItem(ItemType.KeycardO5,0,0,0,0));
                        delay++;
                    }
                    break;

                case KeyCode.Alpha9:
                    Logger.Get.Info(ev.Player.Inventory.Items.Count.ToString());
                    break;

                case KeyCode.U:
                    string text = $"All GameObjects for MapSeed: {Api.Map.Get.Seed}";
                    foreach (var gobject in Synapse.Server.Get.GetObjectsOf<GameObject>())
                        text += $"\n{gobject.name} : Position {gobject.transform.position} Scale: {gobject.transform.localScale}";

                    var path = Path.Combine(Synapse.Server.Get.Files.SynapseDirectory, "GameObjects.txt");

                    if (!File.Exists(path)) File.Create(path).Close();

                    File.WriteAllText(path, text);
                    break;

                case KeyCode.O:
                    ev.Player.Broadcast(5, ev.Player.LookingAt == null ? "Null" : ev.Player.LookingAt.name);
                    break;

                case KeyCode.C:
                    Logger.Get.Info("Try BC");

                    var bc = ev.Player.SendBroadcast(20, "Message");

                    Timing.CallDelayed(5f, () => bc.Message = "Message2!");

                    Timing.CallDelayed(10f, () => ev.Player.SendBroadcast(5, "Message3", true));

                    ev.Player.SendBroadcast(10, "Message4");
                    break;

                case KeyCode.B:
                    Logger.Get.Info(ev.Player.MapPoint.ToString());
                    break;

                case KeyCode.L:
                    var item = new Items.SynapseItem(ItemType.GunCOM15, 0, 0, 0, 0);
                    item.Drop(ev.Player.Position);
                    Timing.CallDelayed(5f, () => {
                        item.Position = ev.Player.Position;
                        item.Scale = Vector3.one * 2;
                        item.Barrel = 1;
                        item.Other = 1;
                    });
                    break;
                case KeyCode.K:
                    var item2 = new Items.SynapseItem(ItemType.Medkit, 0, 0, 0, 0);
                    item2.PickUp(ev.Player);
                    Timing.CallDelayed(5f, () => item2.Drop());
                    break;
                case KeyCode.J:
                    var item3 = new Items.SynapseItem(ItemType.GunCOM15, 10, 0, 0, 0);
                    item3.PickUp(ev.Player);
                    Timing.CallDelayed(5f, () =>
                    {
                        try
                        {
                            item3.Durabillity = 100;
                            item3.Barrel = 1;
                            item3.Other = 1;
                            item3.Sight = 1;
                        }
                        catch(Exception e)
                        {
                            Logger.Get.Error(e.ToString());
                        }
                        
                    });
                    break;

                case KeyCode.H:
                    var item4 = new Items.SynapseItem(ItemType.Medkit, 0, 0, 0,0);
                    item4.Scale = Vector3.one * 5;
                    item4.Drop(ev.Player.Position);

                    Timing.CallDelayed(10f, () =>
                    {
                        item4.PickUp(ev.Player);
                        Timing.CallDelayed(5f, () => item4.Drop());
                    });
                    break;
#if DEBUG
                case KeyCode.G:
                    if(testitem == null)
                    {
                        testitem = new Items.SynapseItem(ItemType.GunCOM15, 10, 0, 0, 0);
                        testitem.Scale = Vector3.one * 3;
                        testitem.PickUp(ev.Player);
                    }
                    else
                    {
                        testitem.Drop();
                        Timing.CallDelayed(5f, () => testitem.PickUp(ev.Player));
                    }
                    break;
#endif

                case KeyCode.Z:
                    foreach (var item5 in SynapseController.Server.Map.Items)
                        item5.Scale = item5.Scale * 2;
                    break;

                case KeyCode.T:
                    var item6 = new Items.SynapseItem(ItemType.Medkit, 0, 0, 0, 0)
                    {
                        Position = ev.Player.Position
                    };
                    item6.Drop();
                    break;
            }
        }
#if DEBUG
        private Items.SynapseItem testitem;
#endif


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

            if (!ev.Player.VanillaItems.Any()) return;
            foreach (var gameItem in ev.Player.VanillaItems.Select(item => ev.Player.VanillaInventory.GetItemByID(item.id)).Where(gameitem => gameitem.permissions != null && gameitem.permissions.Length != 0))
            {
                ev.Allow = gameItem.permissions.Any(p =>
                    global::Door.backwardsCompatPermissions.TryGetValue(p, out var flag) &&
                    ev.Door.PermissionLevels.HasPermission(flag));
            }
        }
#endregion
    }
}
