using Mirror;
using Synapse.Api.CustomObjects;
using Synapse.Config;
using System.Collections.Generic;
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
                case KeyCode.Alpha0:
                    var locker = new SynapseLockerObject(Enum.LockerType.ScpPedestal, ev.Player.Position, Quaternion.identity, Vector3.one);

                    MEC.Timing.CallDelayed(5f,() => locker.Position = ev.Player.Position);
                    break;

                case KeyCode.Alpha1:
                    var dummy = SchematicHandler.Get.SpawnSchematic(new SynapseSchematic
                    {
                        DummyObjects = new System.Collections.Generic.List<SynapseSchematic.DummyConfiguration>
                        {
                            new SynapseSchematic.DummyConfiguration
                            {
                                HeldItem = ItemType.None,
                                Position = Vector3.zero,
                                Name = "",
                                Role = RoleType.ClassD,
                                Rotation = Quaternion.identity,
                                Scale = Vector3.one
                            }
                        }
                    }, ev.Player.Position);
                    break;

                case KeyCode.Alpha2:
                    var dummy2 = new Dummy(ev.Player.Position, Quaternion.identity, RoleType.ClassD, "Shooter");
                    dummy2.HeldItem = ItemType.GunLogicer;
                    var turret = new Turret(ev.Player.Position);

                    MEC.Timing.RunCoroutine(Turret(turret, ev.Player, dummy2));
                    break;
            }
        }

        public IEnumerator<float> Turret(Turret turret, Player player, Dummy dummy)
        {
            dummy.GameObject.transform.parent = turret.GameObject.transform;

            for (; ; )
            {
                if (Vector3.Distance(player.Position, turret.Position) > 50f) yield break;

                dummy.RotateToPosition(player.Position);

                var dir = player.Position - turret.Position;
                turret.SingleShootDirection(dir);

                yield return MEC.Timing.WaitForSeconds(0.2f);
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
