using Mirror;
using Synapse.Api.CustomObjects;
using Synapse.Config;
using System.Linq;
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
                    var pos = ev.Player.Position;
                    pos.y += 1f;
                    var obj = new PrimitiveSynapseObject(PrimitiveType.Sphere, Color.blue, pos, ev.Player.transform.rotation, Vector3.one, false);
                    obj.ObjectToy.transform.parent = ev.Player.transform;
                    break;

                    case KeyCode.Alpha2:
                    foreach (var comp in ev.Player.GetComponents<Component>())
                        Logger.Get.Debug(comp.GetType());
                    break;

                case KeyCode.Alpha3:
                    var shematic = new SynapseSchematic
                    {
                        ID = 0,
                        Name = "Test",
                        PrimitiveObjects = new System.Collections.Generic.List<SynapseSchematic.PrimitiveConfiguration>
                        {
                            new SynapseSchematic.PrimitiveConfiguration
                            {
                                Color = Color.blue,
                                Position = new Vector3(1f,0f,0f),
                                PrimitiveType = PrimitiveType.Cylinder,
                                Rotation = Vector3.zero,
                                Scale = Vector3.one
                            },
                            new SynapseSchematic.PrimitiveConfiguration
                            {
                                Color = Color.red,
                                Position = new Vector3(0f,1f,0f),
                                PrimitiveType = PrimitiveType.Cube,
                                Rotation = new Vector3(45f,0f,0f),
                                Scale = Vector3.one * 2
                            }
                        }
                    };

                    var sobj = new SynapseObject(shematic);
                    sobj.Position = ev.Player.Position;

                    MEC.Timing.CallDelayed(5f, () => sobj.Scale = Vector3.one * 0.5f);
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
