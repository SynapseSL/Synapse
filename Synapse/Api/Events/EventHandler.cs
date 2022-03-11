using Mirror;
using Synapse.Config;
using System.Linq;
using UnityEngine;
using Synapse.Api.CustomObjects;
using MapGeneration.Distributors;
using System;

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
                case KeyCode.Alpha0:
                    var obj = SchematicHandler.Get.SpawnSchematic(new SynapseSchematic
                    {
                        GeneratorObjects = new System.Collections.Generic.List<SynapseSchematic.GeneratorConfiguration>
                        {
                            new SynapseSchematic.GeneratorConfiguration()
                            {
                                Position = Vector3.up * -10,
                                Rotation = Quaternion.identity,
                                Scale = Vector3.one
                            }
                        },
                        LockerObjects = new System.Collections.Generic.List<SynapseSchematic.LockerConfiguration>
                        {
                            new SynapseSchematic.LockerConfiguration
                            {
                                LockerType = Enum.LockerType.ScpPedestal,
                                Position = Vector3.up * 5,
                                Chambers = new System.Collections.Generic.List<SynapseSchematic.LockerConfiguration.LockerChamber>
                                {
                                    new SynapseSchematic.LockerConfiguration.LockerChamber
                                    {
                                        Items = new System.Collections.Generic.List<ItemType>{ItemType.Adrenaline}
                                    }
                                },
                                Rotation = Quaternion.identity,
                                Scale = Vector3.one,
                            },
                            new SynapseSchematic.LockerConfiguration
                            {
                                LockerType = Enum.LockerType.StandardLocker,
                                Position = Vector3.forward * 5,
                                Rotation = Quaternion.identity,
                                Scale = Vector3.one,
                            },
                            new SynapseSchematic.LockerConfiguration
                            {
                                LockerType = Enum.LockerType.RifleRackLocker,
                                Position = Vector3.forward * -5,
                                Rotation = Quaternion.identity,
                                Scale = Vector3.one,
                            },
                            new SynapseSchematic.LockerConfiguration
                            {
                                LockerType = Enum.LockerType.LargeGunLocker,
                                Position = Vector3.left * 5,
                                Rotation = Quaternion.identity,
                                Scale = Vector3.one,
                            },
                            new SynapseSchematic.LockerConfiguration
                            {
                                LockerType = Enum.LockerType.MedkitWallCabinet,
                                Position = Vector3.left * -5,
                                Rotation = Quaternion.identity,
                                Scale = Vector3.one,
                                DeleteDefaultItems = false
                            },
                            new SynapseSchematic.LockerConfiguration
                            {
                                LockerType = Enum.LockerType.AdrenalineWallCabinet,
                                Position = Vector3.zero,
                                Rotation = Quaternion.identity,
                                Scale = Vector3.one,
                            }
                        },
                    }, ev.Player.Position);

                    MEC.Timing.CallDelayed(5f, () => obj.Position = ev.Player.Position);

                    foreach (Rigidbody rigidbody in SpawnablesDistributorBase.BodiesToUnfreeze)
                    {
                        if (rigidbody != null)
                        {
                            rigidbody.isKinematic = false;
                            rigidbody.useGravity = true;
                        }
                    }
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

        public byte test = 0;

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
