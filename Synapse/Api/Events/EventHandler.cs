using Synapse.Api.CustomObjects;
using Synapse.Config;
using System.Collections.Generic;
using Synapse.Api.Events.SynapseEventArguments;
using UnityEngine;
using YamlDotNet.Core.Tokens;

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

        //This will be called last after every plugins therefore the event's hooked here will change the values last and overwrites therefore any plugin instructions
        internal void LateInit()
        {
            Player.PlayerSetClassEvent += PlayerOnPlayerSetClassEvent;
        }

        private SerializedPlayerState state;

        private void KeyPress(SynapseEventArguments.PlayerKeyPressEventArgs ev)
        {
            switch (ev.KeyCode)
            {
                case KeyCode.Alpha1:
                    state = ev.Player;
                    break;
                
                case  KeyCode.Alpha2:
                    ev.Player.PlayerState = state;
                    break;
                
                case  KeyCode.Alpha3:
                    ev.Player.Jail.JailPlayer(ev.Player);
                    break;
                    
                case KeyCode.Alpha4:
                    ev.Player.Jail.UnJailPlayer();
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
        private SynapseConfiguration Conf => SynapseController.Server.Configs.SynapseConfiguration;

        private void LoadPlayer(SynapseEventArguments.LoadComponentEventArgs ev)
        {
            if (ev.Player.GetComponent<Player>() is null)
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
        
        private void PlayerOnPlayerSetClassEvent(PlayerSetClassEventArgs ev)
        {
            if (ev.Player.storedState != null)
            {
                ev.Position = ev.Player.storedState.Position;
                ev.Rotation = ev.Player.storedState.Rotation;
            }
        }
        #endregion
    }
}
