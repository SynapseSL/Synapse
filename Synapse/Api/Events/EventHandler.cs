using Synapse.Config;
using UnityEngine;
using System.Linq;
using MEC;
using RemoteAdmin;
using System.Collections.Generic;
using System;

namespace Synapse.Api.Events
{
    public class EventHandler
    {
        internal EventHandler()
        {
            Player.PlayerJoinEvent += PlayerJoin;
            Player.PlayerSyncDataEvent += PlayerSyncData;
#if DEBUG
            Player.PlayerKeyPressEvent += KeyPress;
#endif
        }

        private void KeyPress(SynapseEventArguments.PlayerKeyPressEventArgs ev)
        {
            switch (ev.KeyCode)
            {
                case KeyCode.Alpha1:
                    var dummy = new Dummy(ev.Player.Position, ev.Player.transform.rotation, ev.Player.RoleType);
                    break;
            }
        }

        private IEnumerator<float> Walk(Dummy dummy, float speed)
        {
            yield return Timing.WaitForSeconds(1f);

            dummy.Player.AnimationController.Networkspeed = new Vector2(speed, 0f);
            dummy.Player.AnimationController.Network_curMoveState = (byte)PlayerMovementState.Walking;
            for (; ; )
            {
                try
                {
                    var pos = dummy.Position + dummy.Player.CameraReference.forward / 10 * speed;
                    if (!Physics.Linecast(dummy.Position, pos, dummy.Player.PlayerMovementSync.CollidableSurfaces))
                    {
                        dummy.Player.PlayerMovementSync.OverridePosition(pos, dummy.Player.PlayerMovementSync.Rotations.y, true);
                    }
                }
                catch (Exception e)
                {
                    Logger.Get.Error(e);
                }
                yield return Timing.WaitForSeconds(0.1f);
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
        private SynapseConfiguration Conf => SynapseController.Server.Configs.synapseConfiguration;

        private void PlayerJoin(SynapseEventArguments.PlayerJoinEventArgs ev)
        {
            ev.Player.Broadcast(Conf.JoinMessagesDuration, Conf.JoinBroadcast);
            ev.Player.GiveTextHint(Conf.JoinTextHint, Conf.JoinMessagesDuration);
            if (!string.IsNullOrWhiteSpace(Conf.JoinWindow))
                ev.Player.OpenReportWindow(Conf.JoinWindow.Replace("\\n","\n"));
        }

        private void PlayerSyncData(SynapseEventArguments.PlayerSyncDataEventArgs ev)
        {
            if (ev.Player.RoleType != RoleType.ClassD &&
                ev.Player.RoleType != RoleType.Scientist &&
                !(Vector3.Distance(ev.Player.Position, ev.Player.GetComponent<Escape>().worldPosition) >= Escape.radius))
                ev.Player.ClassManager.CmdRegisterEscape();
        }
#endregion
    }
}
