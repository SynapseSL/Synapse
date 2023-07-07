using System;
using CustomPlayerEffects;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using Neuron.Core.Meta;
using PluginAPI.Enums;
using PluginAPI.Events;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Player;
using Utils.Networking;

namespace Synapse3.SynapseModule.Mirror.Events;

public class MirrorItemEvents : Service
{
#if !PATCHLESS
    private readonly RoundEvents _roundEvents;
    private readonly ItemEvents _itemEvents;
    private readonly PlayerService _playerService;

    public MirrorItemEvents(RoundEvents roundEvents, ItemEvents itemEvents, PlayerService playerService)
    {
        _roundEvents = roundEvents;
        _itemEvents = itemEvents;
        _playerService = playerService;
    }
    
    public override void Enable()
    {
        _roundEvents.Waiting.Subscribe(Waiting);
    }

    public override void Disable()
    {
        _roundEvents.Waiting.Unsubscribe(Waiting);
    }

    private void Waiting(RoundWaitingEvent ev)
    {
        NetworkServer.ReplaceHandler<RequestMessage>(OnWeaponRequestMessage);
        NetworkServer.ReplaceHandler<ShotMessage>(OnShotMessage);
        //NetworkServer.ReplaceHandler<ThrowableNetworkHandler.ThrowableItemRequestMessage>(OnThrowMessage);
    }

    private void OnThrowMessage(NetworkConnection connection,
        ThrowableNetworkHandler.ThrowableItemRequestMessage message)
    {
        try
        {
            var player = connection.GetSynapsePlayer();
            if (player == null) return;
            var item = player.Inventory.ItemInHand;
            if (item.Serial != message.Serial) return;
            if (item.Item is not ThrowableItem throwableItem) return;

            switch (message.Request)
            {
                case ThrowableNetworkHandler.RequestType.BeginThrow:
                    var ev = new ThrowGrenadeEvent(item, ItemInteractState.Start, player, false);
                    _itemEvents.ThrowGrenade.RaiseSafely(ev);
                    if (!ev.Allow)
                    {
                        ForceStopGrenade(throwableItem, player);
                        return;
                    }
                    throwableItem.ServerProcessInitiation();
                    break;
                
                case ThrowableNetworkHandler.RequestType.ConfirmThrowWeak:
                case ThrowableNetworkHandler.RequestType.ConfirmThrowFullForce:
                    ev = new ThrowGrenadeEvent(item, ItemInteractState.Finalize, player,
                        message.Request == ThrowableNetworkHandler.RequestType.ConfirmThrowFullForce);
                    _itemEvents.ThrowGrenade.RaiseSafely(ev);
                    if (!ev.Allow)
                    {
                        ForceStopGrenade(throwableItem, player);
                        return;
                    }

                    throwableItem.ServerProcessThrowConfirmation(ev.ThrowFullForce, message.CameraPosition.Position,
                        message.CameraRotation, message.PlayerVelocity);
                    break;
                
                case ThrowableNetworkHandler.RequestType.CancelThrow:
                    ev = new ThrowGrenadeEvent(item, ItemInteractState.Cancel, player, false);
                    _itemEvents.ThrowGrenade.RaiseSafely(ev);
                    throwableItem.ServerProcessCancellation();
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Warn("Error during handling of Player ThrowableItem Request Message\n" + ex);
            ThrowableNetworkHandler.ServerProcessRequest(connection, message);
        }
    }

    private void ForceStopGrenade(ThrowableItem throwable, SynapsePlayer player)
    {
        throwable.CancelStopwatch.Start();
        throwable.ThrowStopwatch.Reset();
        ReCreateItem(player, player.Inventory.ItemInHand);
        new ThrowableNetworkHandler.ThrowableItemAudioMessage(throwable.ItemSerial, ThrowableNetworkHandler.RequestType.CancelThrow).SendToAuthenticated();
    }
    
    private static void ReCreateItem(SynapsePlayer player, SynapseItem item)
    {
        var newItem = new SynapseItem(item.Id)
        {
            Durability = item.Durability,
            ObjectData = item.ObjectData,
            Scale = item.Scale,
            Parent = item.Parent,
            OriginalScale = item.OriginalScale,
            UpgradeProcessors = item.UpgradeProcessors,
            MoveInElevator = item.MoveInElevator,
            SchematicConfiguration = item.SchematicConfiguration,
            CanBePickedUp = item.CanBePickedUp
        };
        item.Destroy();
        newItem.EquipItem(player);
    }

    private void OnWeaponRequestMessage(NetworkConnection connection, RequestMessage message)
    {
        try
        {
            if (connection?.identity?.gameObject == null) return;
            if (message.Request != RequestType.Reload)
            {
                FirearmBasicMessagesHandler.ServerRequestReceived(connection, message);
                return;
            }

            var player = connection.GetSynapsePlayer();
            if (player == null) return;
            var item = player.Inventory.ItemInHand;
            if (message.Serial != item.Serial) return;
            if (item.Item is not Firearm firearm) return;

            var allow = EventManager.ExecuteEvent(ServerEventType.PlayerReloadWeapon, player.Hub, firearm) &&
                        firearm.AttachmentsValue(AttachmentParam.PreventReload) <= 0f;

            var ev = new ReloadWeaponEvent(item, ItemInteractState.Finalize, player, false)
            {
                Allow = allow
            };
            _itemEvents.ReloadWeapon.RaiseSafely(ev);

            if ((ev.Allow && firearm.AmmoManagerModule.ServerTryReload()) || ev.PlayAnimationOverride)
                message.SendToAuthenticated();
        }
        catch (Exception ex)
        {
            Logger.Warn("Error during handling of Player Weapon Request Message\n" + ex);
            FirearmBasicMessagesHandler.ServerRequestReceived(connection, message);
        }
    }

    private void OnShotMessage(NetworkConnection connection, ShotMessage message)
    {
        try
        {
            var player = connection.GetSynapsePlayer();
            if (player == null) return;
            if (player.Inventory.ItemInHand.Serial != message.ShooterWeaponSerial) return;
            if (player.Inventory.ItemInHand.Item is not Firearm firearm) return;
            if (!firearm.ActionModule.ServerAuthorizeShot()) return;

            var target = message.TargetNetId == 0 ? null : _playerService.GetPlayer(message.TargetNetId);
            var ev = new ShootEvent(player.Inventory.ItemInHand, ItemInteractState.Finalize, player, target)
            {
                Allow = true
            };
            Synapse.Get<ItemEvents>().Shoot.RaiseSafely(ev);
            if (!ev.Allow) return;

            if (SpawnProtected.CheckPlayer(player.Hub) && !SpawnProtected.CanShoot)
                player.PlayerEffectsController.DisableEffect<SpawnProtected>();
            firearm.HitregModule.ServerProcessShot(message);
            firearm.OnWeaponShot();
        }
        catch (Exception ex)
        {
            Logger.Warn("Error during handling of Player Weapon Request Message\n" + ex);
            FirearmBasicMessagesHandler.ServerShotReceived(connection, message);
        }
    }
#endif
}