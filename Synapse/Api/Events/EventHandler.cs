using Mirror;
using Synapse.Api.CustomObjects;
using Synapse.Config;
using UnityEngine;

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
                                Position = new Vector3(0f,-1f,0f),
                                PrimitiveType = PrimitiveType.Capsule,
                                Rotation = Vector3.zero,
                                Scale = Vector3.one * 2
                            },
                            new SynapseSchematic.PrimitiveConfiguration
                            {
                                Color = Color.gray,
                                Position = new Vector3(0f,2f,0f),
                                PrimitiveType = PrimitiveType.Cube,
                                Rotation = new Vector3(45f,45f,45f),
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
                                Position = new Vector3(1f,0f,0f),
                                Rotation = Vector3.zero,
                                Scale = Vector3.one
                            }
                        }
                    };
                    var sobj = SchematicHandler.Get.SpawnSchematic(schematic, ev.Player.Position);

                    SchematicHandler.Get.SaveSchematic(schematic, "Key3");
                    break;

                case KeyCode.Alpha3:
                    var obj = new SynapseLightObject(Color.green, 1f, 10f, true, ev.Player.Position, ev.Player.transform.rotation, Vector3.one);

                    MEC.Timing.CallDelayed(5f, () => obj.LightShadows = false);
                    break;

                case KeyCode.Alpha4:
                    new SynapseTargetObject(Enum.TargetType.DBoy, ev.Player.Position, ev.Player.transform.rotation, Vector3.one * 3);
                    break;

                case KeyCode.Alpha5:
                    new SynapseTargetObject(Enum.TargetType.Binary, ev.Player.Position, ev.Player.transform.rotation, Vector3.one * 3);
                    break;

                case KeyCode.Alpha6:
                    new SynapseTargetObject(Enum.TargetType.Sport, ev.Player.Position, ev.Player.transform.rotation, Vector3.one * 3);
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
