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
            Player.PlayerItemUseEvent += Player_PlayerItemUseEvent;
            Map.GeneratorEngageEvent += Map_GeneratorEngageEvent;
#endif
        }

        private int count = 0;

        private void Map_GeneratorEngageEvent(SynapseEventArguments.GeneratorEngageEventArgs ev)
        {
            count++;
            ev.Allow = false;

            Logger.Get.Debug("ENGAGE");
            if (count == 51)
                ev.ResetTime();

            if (count == 100)
                ev.Deactivate();

            if (count == 200)
                ev.Deactivate();
        }

        private void Player_PlayerItemUseEvent(SynapseEventArguments.PlayerItemInteractEventArgs ev)
        {
            if (ev.CurrentItem.ItemType == ItemType.SCP018 && ev.State == SynapseEventArguments.ItemInteractState.Initiating)
                ev.Allow = false;

            if (ev.CurrentItem.ItemType == ItemType.GrenadeHE && ev.State == SynapseEventArguments.ItemInteractState.Finalizing)
                ev.Allow = false;

            if (ev.CurrentItem.ItemType == ItemType.GrenadeFlash && ev.State == SynapseEventArguments.ItemInteractState.Stopping)
                ev.Allow = false;
        }

        private void KeyPress(SynapseEventArguments.PlayerKeyPressEventArgs ev)
        {
            switch (ev.KeyCode)
            {
                case KeyCode.Alpha1:
                    var schematic = SchematicHandler.Get.SpawnSchematic(new SynapseSchematic()
                    {
                        PrimitiveObjects = new List<SynapseSchematic.PrimitiveConfiguration>
                        {
                            new SynapseSchematic.PrimitiveConfiguration
                            {
                                PrimitiveType = PrimitiveType.Cube,
                                Scale = new SerializedVector3(1f,2f,0.01f),
                                Position = Vector3.zero,
                                Rotation = Quaternion.identity,
                                Color = Color.white
                                
                            }
                        },
                        CustomAttributes = new List<string>
                        {
                            "MapTeleporter:1:Outside:0:-45:0"
                        }
                    }, ev.Player.Position);
                    break;

                case KeyCode.Alpha2:
                    Items.ItemManager.Get.SetSchematicForVanillaItem(ItemType.Coin, new SynapseSchematic
                    {
                        PrimitiveObjects = new List<SynapseSchematic.PrimitiveConfiguration>
                        {
                            new SynapseSchematic.PrimitiveConfiguration
                            {
                                PrimitiveType = PrimitiveType.Sphere,
                                Color = Color.blue,
                                Position = Vector3.zero,
                                Rotation = Quaternion.identity,
                                Scale = Vector3.one * 0.1f,
                            },
                           
                        },
                        ItemObjects = new List<SynapseSchematic.ItemConfiguration>
                        {
                            new SynapseSchematic.ItemConfiguration
                            {
                                ItemType = ItemType.Medkit,
                                CanBePickedUp = true,
                                Rotation = Quaternion.identity,
                                Scale = Vector3.one * 0.5f,
                                Position = Vector3.up * 0.7f,
                                Attachments = 0,
                                Durabillity = 0,
                            }
                        }
                    });
                    break;

                case KeyCode.Alpha3:
                    var turret = new Turret(ev.Player.Position);
                    turret.ShootAutomatic = true;
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
