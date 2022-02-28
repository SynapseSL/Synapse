using Mirror;
using Synapse.Api.CustomObjects;
using Synapse.Config;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Synapse.Api.Items;
using MEC;

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
                    var schematic = new SynapseSchematic
                    {
                        ID = 3,
                        Name = "test",
                        PrimitiveObjects = new System.Collections.Generic.List<SynapseSchematic.PrimitiveConfiguration>
                        {
                            new SynapseSchematic.PrimitiveConfiguration
                            {
                                Color = Color.red,
                                Position = new Vector3(0f, -1f, 0f),
                                PrimitiveType = PrimitiveType.Capsule,
                                Rotation = Vector3.zero,
                                Scale = Vector3.one * 2
                            },
                            new SynapseSchematic.PrimitiveConfiguration
                            {
                                Color = Color.gray,
                                Position = new Vector3(0f, 2f, 0f),
                                PrimitiveType = PrimitiveType.Cube,
                                Rotation = new Vector3(45f, 45f, 45f),
                                Scale = Vector3.one
                            },
                        },
                        LightObjects = new System.Collections.Generic.List<SynapseSchematic.LightSourceConfiguration>
                        {
                            new SynapseSchematic.LightSourceConfiguration
                            {
                                Color = Color.green,
                                LightIntensity = 1,
                                LightRange = 100,
                                LightShadows = true,
                                Position = new Vector3(1f, 0f, 0f),
                                Rotation = Vector3.zero,
                                Scale = Vector3.one
                            }
                        },
                        TargetObjects = new System.Collections.Generic.List<SynapseSchematic.TargetConfiguration>
                        {
                            new SynapseSchematic.TargetConfiguration
                            {
                                Position = new Vector3(3f, 0f, 0f),
                                Rotation = Vector3.zero,
                                Scale = Vector3.one
                            }
                        },
                        ItemObjects = new System.Collections.Generic.List<SynapseSchematic.ItemConfiguration>
                        {
                            new SynapseSchematic.ItemConfiguration
                            {
                                ItemType = ItemType.MicroHID,
                                Position = new Vector3(0f, 5f, 0f),
                                Rotation = Vector3.zero,
                                Scale = Vector3.one * 4,
                                CanBePickedUp = true,
                            }
                        },
                        WorkStationObjects = new System.Collections.Generic.List<SynapseSchematic.WorkStationConfiguration>
                        {
                            new SynapseSchematic.WorkStationConfiguration
                            {
                                Position = new Vector3(-2f, 0f, 0f),
                                Rotation = Vector3.zero,
                                Scale = Vector3.one,
                                UpdateEveryFrame = true
                            }
                        },
                        DoorObjects = new System.Collections.Generic.List<SynapseSchematic.DoorConfiguration>
                        {
                            new SynapseSchematic.DoorConfiguration
                            {
                                Position = new Vector3(0f,0f,-3f),
                                Rotation = Vector3.zero,
                                Scale = Vector3.one * 2,
                                DoorType = Enum.SpawnableDoorType.LCZ,
                                Locked = true,
                                Open = true,
                                UpdateEveryFrame = true
                            }
                        }
                    };
                    var sobj = SchematicHandler.Get.SpawnSchematic(schematic, ev.Player.Position);
                    MEC.Timing.CallDelayed(5f, () => sobj.Scale = Vector3.one * 0.5f);
                    MEC.Timing.CallDelayed(10f, () => sobj.ApplyPhysics());
                    SchematicHandler.Get.SaveSchematic(schematic, "Key2");
                    break;

                case KeyCode.Alpha3:
                    foreach(var item in (ItemType[])System.Enum.GetValues(typeof(ItemType)))
                    {
                        ItemManager.Get.SetSchematicForVanillaItem(item, new SynapseSchematic
                        {
                            ID = 99,
                            Name = "CustomItem",
                            PrimitiveObjects = new System.Collections.Generic.List<SynapseSchematic.PrimitiveConfiguration>
                            {
                                new SynapseSchematic.PrimitiveConfiguration
                                {
                                    Color = Color.red,
                                    Position = Vector3.up,
                                    PrimitiveType = PrimitiveType.Sphere,
                                    Rotation = Vector3.zero,
                                    Scale = Vector3.one * 0.1f
                                }
                            },
                            ItemObjects = new System.Collections.Generic.List<SynapseSchematic.ItemConfiguration>
                            {
                                new SynapseSchematic.ItemConfiguration
                                {
                                    CanBePickedUp = false,
                                    ItemType = ItemType.Medkit,
                                    Scale = Vector3.one * 0.1f,
                                    Position = Vector3.zero,
                                    Rotation = Vector3.zero,
                                }
                            }
                        });
                    }
                    break;

                case KeyCode.Alpha5:
                    foreach (var item in SynapseController.Server.Map.Items)
                        item.Scale = Vector3.one * 3;
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

                case KeyCode.Alpha6:
                    Timing.RunCoroutine(Test(ev.Player));
                    break;

                case KeyCode.Alpha7:
                    foreach (var obj in SynapseController.Server.Map.Rooms)
                    {
                        var comp = obj.GameObject.GetComponentInParent<NetworkIdentity>();
                        comp?.DespawnForOnePlayer(ev.Player);
                    }
                        break;

                case KeyCode.Alpha8:
                    foreach (var room in SynapseController.Server.Map.Rooms)
                        room.Scale = Vector3.one * 2;
                    break;

                case KeyCode.Alpha9:
                    foreach (var obj in GameObject.FindObjectsOfType<NetworkIdentity>())
                        if (obj.name.Contains("All"))
                        {
                            if (Vector3.Distance(obj.transform.position, ev.Player.Position) < 10f)
                                Logger.Get.Debug($"Name: {obj.name} Asset{obj.assetId} Net{obj.netId}");
                        }
                    break;
            }
        }

        private IEnumerator<float> Test(Player player)
        {
            foreach(var obj in GameObject.FindObjectsOfType<NetworkIdentity>())
            {
                if (!obj.name.Contains("All")) continue;
                Logger.Get.Debug(obj.assetId);
                obj.GetComponent<NetworkIdentity>()?.DespawnForOnePlayer(player);
                yield return Timing.WaitForSeconds(2f);
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
