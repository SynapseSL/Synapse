using Mirror;
using Synapse.Config;
using System.Linq;
using UnityEngine;
using Synapse.Api.CustomObjects;

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
                    foreach (var pref in NetworkManager.singleton.spawnPrefabs)
                        Logger.Get.Debug(pref.name);
                    break;

                case KeyCode.Alpha2:
                    SchematicHandler.Get.SpawnSchematic(new SynapseSchematic
                    {
                        ID = 3672,
                        Name = "weqeqwe",
                        PrimitiveObjects = new System.Collections.Generic.List<SynapseSchematic.PrimitiveConfiguration>
                        {
                            new SynapseSchematic.PrimitiveConfiguration
                            {
                                Color = Color.red,
                                Position = Vector3.zero,
                                PrimitiveType = PrimitiveType.Cube,
                                Rotation = Vector3.zero,
                                Scale = Vector3.one
                            }
                        },
                        DummyObjects = new System.Collections.Generic.List<SynapseSchematic.DummyConfiguration>
                        {
                            new SynapseSchematic.DummyConfiguration
                            {
                                HeldItem = ItemType.None,
                                Scale = Vector3.one,
                                Name = "Dummy",
                                Position = Vector3.zero,
                                Rotation = Quaternion.identity,
                                Role = RoleType.ClassD
                            }
                        }
                    }, ev.Player.Position);
                    break;

                case KeyCode.Alpha3:
                    var dum = new SynapseDummyObject(ev.Player.Position, ev.Player.transform.rotation, Vector3.one, ev.Player.RoleType, ItemType.Coin, "asd", "asdsds", "cyan");
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

                case KeyCode.Alpha8:
                    foreach (var room in SynapseController.Server.Map.Rooms)
                        room.Rotation = Quaternion.Euler(180f, 0f, 0f);
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

                foreach (var role in CharacterClassManager._staticClasses)
                    if (role != null)
                        SynapseRagdollObject.Prefabs[role.roleId] = role.model_ragdoll;
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
