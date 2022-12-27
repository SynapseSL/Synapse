using System;
using System.Linq;
using System.Reflection.Emit;
using CommandSystem;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Pickups;
using LightContainmentZoneDecontamination;
using LiteNetLib;
using MapGeneration.Distributors;
using Neuron.Core.Logging;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp049;
using PlayerStatsSystem;
using PluginAPI.Core.Attributes;
using PluginAPI.Core.Interfaces;
using PluginAPI.Core.Zones.Heavy;
using PluginAPI.Enums;
using PluginAPI.Events;
using RemoteAdmin;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using UnityEngine;
using static RoundSummary;

namespace Synapse3.SynapseModule.Events;

public partial class PlayerEvents
{

    [PluginEvent(ServerEventType.PlayerChangeRole)]
    public bool PlayerChangeRoleHook(IPlayer player, PlayerRoleBase oldRole, RoleTypeId newRole, RoleChangeReason changeReason)
    {
        NeuronLogger.For<Synapse>().Warn("PlayerChangeRoleHook");

        var ev = new SetClassEvent(player.GetSynapsePlayer(), newRole, changeReason);

        SetClass.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerInteractDoor)]
    public bool PlayerInteractDoorHook(IPlayer player, DoorVariant door, bool canOpen)
    {
        var synapsePlayer = player.GetSynapsePlayer();
        var synapseDoor = door.GetSynapseDoor();
        var useByPass = synapseDoor.Locked ? synapsePlayer.Bypass : false;

        var ev = new DoorInteractEvent(synapsePlayer, canOpen, synapseDoor, useByPass);

        DoorInteract.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerInteractLocker)]
    public bool PlayerInteractLockerHook(IPlayer player, Locker locker, byte colliderId, bool canOpen)
    {
        var synapseLocker = locker.GetSynapseLocker();
        var chamber = synapseLocker.Chambers.FirstOrDefault(p => p.ByteID == colliderId);

        var ev = new LockerUseEvent(player.GetSynapsePlayer(), canOpen, synapseLocker, chamber);

        LockerUse.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.WarheadStart)]
    public bool WarheadStartHook(bool isAutomatic, IPlayer player)
    {
        var ev = new StartWarheadEvent(player?.GetSynapsePlayer(), true);

        StartWarhead.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerChangeItem)]
    public bool PlayerChangeItemHook(IPlayer player, ushort oldItem, ushort newItem)
    {
        var synapseNewItem = _item.GetSynapseItem(newItem);
        var ev = new ChangeItemEvent(player.GetSynapsePlayer(), true, synapseNewItem);

        ChangeItem.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerDamage)]
    public bool PlayerDamageHook(IPlayer player, IPlayer target, DamageHandlerBase damageHandler)
    {
        var dommageType = damageHandler.GetDamageType();
        var dommageAmount = damageHandler is StandardDamageHandler standard ? standard.Damage : -1;

        var ev = new DamageEvent(player.GetSynapsePlayer(), true, target?.GetSynapsePlayer(), dommageType, dommageAmount);

        Damage.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerRemoveHandcuffs)]
    public bool PlayerInteractShootingTargetHook(IPlayer player, IPlayer target)
    {
        var ev = new FreePlayerEvent(player.GetSynapsePlayer(), true, target.GetSynapsePlayer());

        FreePlayer.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerDropAmmo)]
    public bool PlayerDropAmmoHook(IPlayer player, ItemType item, int amount)
    {
        var ev = new DropAmmoEvent(player.GetSynapsePlayer(), true, (AmmoType)item, (ushort)amount);

        DropAmmo.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerUnlockGenerator)]
    public bool PlayerUnlockGeneratorHook(IPlayer player, Scp079Generator generator)
    {
        var ev = new GeneratorInteractEvent(player.GetSynapsePlayer(), true,
            generator.GetSynapseGenerator(), Enums.GeneratorInteract.UnlockDoor);

        GeneratorInteract.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerOpenGenerator)]
    public bool PlayerOpenGeneratorHook(IPlayer player, Scp079Generator generator)
    {
        var ev = new GeneratorInteractEvent(player.GetSynapsePlayer(), true,
            generator.GetSynapseGenerator(), Enums.GeneratorInteract.OpenDoor);

        GeneratorInteract.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerDeactivatedGenerator)]
    public bool PlayerDeactivatedGeneratorHook(IPlayer player, Scp079Generator generator)
    {
        var ev = new GeneratorInteractEvent(player.GetSynapsePlayer(), true,
            generator.GetSynapseGenerator(), Enums.GeneratorInteract.Cancel);

        GeneratorInteract.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerCloseGenerator)]
    public bool PlayerCloseGeneratorHook(IPlayer player, Scp079Generator generator)
    {
        var ev = new GeneratorInteractEvent(player.GetSynapsePlayer(), true,
            generator.GetSynapseGenerator(), Enums.GeneratorInteract.CloseDoor);

        GeneratorInteract.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerActivateGenerator)]
    public bool PlayerActivateGeneratorHook(IPlayer player, Scp079Generator generator)
    {
        var ev = new GeneratorInteractEvent(player.GetSynapsePlayer(), true,
            generator.GetSynapseGenerator(), Enums.GeneratorInteract.Activate);

        GeneratorInteract.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerLeft)]
    public void PlayerLeftHook(IPlayer player)
    {
        var ev = new LeaveEvent(player.GetSynapsePlayer());

        Leave.RaiseSafely(ev);
    }

    [PluginEvent(ServerEventType.PlayerSearchedPickup)]//TODO: Test if that do the Ammo
    public bool PlayerSearchedPickupHook(IPlayer player, ItemPickupBase item)
    {
        var ev = new PickupEvent(player.GetSynapsePlayer(), true, item.GetItem());

        Pickup.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerReport)]
    public bool PlayerReportHook(IPlayer player, IPlayer target, string reason)
    {
        var ev = new ReportEvent(player.GetSynapsePlayer(), true, target.GetSynapsePlayer(), reason, false);

        Report.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerCheaterReport)]
    public bool PlayerCheaterReportHook(IPlayer player, IPlayer target, string reason)
    {
        var ev = new ReportEvent(player.GetSynapsePlayer(), true, target.GetSynapsePlayer(), reason, false);

        Report.RaiseSafely(ev);

        return ev.Allow;
    }


    [PluginEvent(ServerEventType.PlayerKicked)]
    public bool PlayerKickedHook(IPlayer player, IPlayer issuer, string reason)
    {
        var ev = new KickEvent(player?.GetSynapsePlayer(), issuer?.GetSynapsePlayer(), reason, true);

        Kick.RaiseSafely(ev);
        return ev.Allow;
    }


    [PluginEvent(ServerEventType.PlayerBanned)]
    public bool PlayerBandHook(IPlayer player, ICommandSender issuer, string reason, long duration)
    {
        var playerIssuer = (issuer as PlayerCommandSender)?.GetSynapsePlayer();

        var ev = new BanEvent(player?.GetSynapsePlayer(), true, playerIssuer, reason, duration, false);

        Ban.RaiseSafely(ev);
        return ev.Allow;
    }
}

public partial class RoundEvents
{
    private bool _firstTime = true;
    [PluginEvent(ServerEventType.WaitingForPlayers)]
    public void RoundWaitingHook()
    {
        NeuronLogger.For<Synapse>().Warn("RoundWaitingHook");
        Waiting.RaiseSafely(new RoundWaitingEvent(_firstTime));

        _firstTime = false;
    }

    [PluginEvent(ServerEventType.RoundStart)]
    public void RoundStartHook() => Start.RaiseSafely(new RoundStartEvent());

    [PluginEvent(ServerEventType.RoundEnd)]
    public void RoundEndHook(LeadingTeam leadingTeam) => End.RaiseSafely(new RoundEndEvent(leadingTeam));

    [PluginEvent(ServerEventType.RoundRestart)]
    public void RoundRestartHook() => Restart.RaiseSafely(new RoundRestartEvent());

    [PluginEvent(ServerEventType.LczDecontaminationStart)]
    public bool DecontaminationHook()
    {
        var ev = new DecontaminationEvent();
        Decontamination.RaiseSafely(ev);
        if (!ev.Allow)
        {
            DecontaminationController.Singleton.NetworkDecontaminationOverride =
                DecontaminationController.DecontaminationStatus.None;
        }
        return ev.Allow;
    }
}

public partial class ServerEvents
{
    [PluginEvent(ServerEventType.PlayerPreauth)]
    public PreauthCancellationData PreAuth(string userId, string address, long number,
        CentralAuthPreauthFlags centralFlags, string countryCode, byte[] array, ConnectionRequest request, int position)
    {
        var ev = new PreAuthenticationEvent(userId, address, countryCode, centralFlags);
        PreAuthentication.RaiseSafely(ev);
        return ev.ReturningData;
    }
}

public partial class ScpEvents
{
    [PluginEvent(ServerEventType.Scp049ResurrectBody)]
    public bool Scp049ResurrectBodyHook(IPlayer player, IPlayer target, BasicRagdoll body)
    {
        var synapse049 = player.GetSynapsePlayer();
        var synapsetargert = target.GetSynapsePlayer();
        var ragdoll = body.GetSynapseRagdoll();

        var ev = new Scp049ReviveEvent(synapse049, synapsetargert, ragdoll, true);

        Scp049Revive.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.Scp049StartResurrectingBody)]
    public bool Scp049StartResurrectingBodyHook(IPlayer player, IPlayer target, BasicRagdoll body, bool canResurrct)
    {
        var synapse049 = player.GetSynapsePlayer();
        var synapsetargert = target.GetSynapsePlayer();
        var ragdoll = body.GetSynapseRagdoll();

        var ev = new Scp049ReviveEvent(synapse049, synapsetargert, ragdoll, false);

        Scp049Revive.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.Scp173CreateTantrum)]
    public bool Scp173CreateTantrumHook(IPlayer player)
    {
        var synapse173 = player.GetSynapsePlayer();

        var ev = new Scp173PlaceTantrumEvent(synapse173);

        Scp173PlaceTantrum.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.Scp173BreakneckSpeeds)]
    public bool Scp173BreakneckSpeedsHook(IPlayer player, bool activate)
    {
        var synapse173 = player.GetSynapsePlayer();

        var ev = new Scp173ActivateBreakneckSpeedEvent(synapse173, activate);

        Scp173ActivateBreakneckSpeed.RaiseSafely(ev);

        return ev.Allow;
    }
}

public partial class ItemEvents
{
    [PluginEvent(ServerEventType.PlayerHandcuff)]
    public bool PlayerCuffHook(IPlayer player, IPlayer target)
    {
        var synapsePlayer = player.GetSynapsePlayer();
        var synapseTarget = target.GetSynapsePlayer();
        var ev = new DisarmEvent(synapsePlayer.Inventory.ItemInHand, ItemInteractState.Finalize, synapsePlayer, synapseTarget);

        Disarm.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerCoinFlip)]
    public PlayerPreCoinFlipCancellationData.CoinFlipCancellation PlayerFlipCoinHook(IPlayer player, bool IsTails)
    {
        var synapsePlayer = player.GetSynapsePlayer();
        var synapseItem = synapsePlayer.Inventory.ItemInHand;
        var ev = new FlipCoinEvent(synapseItem, synapsePlayer, IsTails);
        FlipCoin.RaiseSafely(ev);
        
        if (!ev.Allow)
            return PlayerPreCoinFlipCancellationData.CoinFlipCancellation.PreventFlip;
        if(ev.Tails)
            return PlayerPreCoinFlipCancellationData.CoinFlipCancellation.Tails;
        else
            return PlayerPreCoinFlipCancellationData.CoinFlipCancellation.Heads;
    }
}

public partial class MapEvents
{
    [PluginEvent(ServerEventType.GeneratorActivated)]
    public bool PlayerActiveGeneratorHook(Scp079Generator generator)
    {
        var synapseGenerator = generator.GetSynapseGenerator();
        var ev = new GeneratorEngageEvent(synapseGenerator);

        GeneratorEngage.RaiseSafely(ev);

        return ev.Allow;
    }

    [PluginEvent(ServerEventType.WarheadStop)]
    public bool PlayerCancelWarHead(IPlayer player)
    {
        var synapsePlayer = player.GetSynapsePlayer();
        var ev = new CancelWarheadEvent(synapsePlayer, true);

        CancelWarhead.RaiseSafely(ev);

        return ev.Allow;
    }

}