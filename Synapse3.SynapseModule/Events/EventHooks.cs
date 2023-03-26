using LightContainmentZoneDecontamination;
using LiteNetLib;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core.Attributes;
using PluginAPI.Core.Interfaces;
using PluginAPI.Enums;
using PluginAPI.Events;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Item;
using UnityEngine;

namespace Synapse3.SynapseModule.Events;

public partial class PlayerEvents
{
    [PluginEvent(ServerEventType.WarheadStart)]
    public bool WarheadStartHook(bool isAutomatic, IPlayer player, bool isResumed)
    {
        if (player == null) return true;
        var ev = new StartWarheadEvent(player?.GetSynapsePlayer(), true, isResumed);
        StartWarhead.RaiseSafely(ev);
        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerDamage)]
    public bool PlayerDamageHook(IPlayer player, IPlayer target, DamageHandlerBase damageHandler)
    {
        if (player == null) return true;
        var damageAmount = damageHandler is StandardDamageHandler standard ? standard.Damage : -1;
        var ev = new DamageEvent(player.GetSynapsePlayer(), true, target?.GetSynapsePlayer(),
            damageHandler.GetDamageType(), damageAmount);
        Damage.RaiseSafely(ev);
        if (damageHandler is StandardDamageHandler standardDamageHandler) standardDamageHandler.Damage = ev.Damage;
        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerRemoveHandcuffs)]
    public bool PlayerFreeHook(IPlayer player, IPlayer target)
    {
        var ev = new FreePlayerEvent(player.GetSynapsePlayer(), true, target.GetSynapsePlayer());
        FreePlayer.RaiseSafely(ev);
        return ev.Allow;
    }
}

public partial class RoundEvents
{
    private bool _firstTime = true;

    [PluginEvent(ServerEventType.WaitingForPlayers)]
    public void RoundWaitingHook()
    {
        Waiting.RaiseSafely(new RoundWaitingEvent(_firstTime));

        _firstTime = false;
    }

    [PluginEvent(ServerEventType.RoundStart)]
    public void RoundStartHook() => Start.RaiseSafely(new RoundStartEvent());

    [PluginEvent(ServerEventType.RoundEnd)]
    public void RoundEndHook(RoundSummary.LeadingTeam leadingTeam) =>
        End.RaiseSafely(new RoundEndEvent(RoundSummary.LeadingTeam.Draw));

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

    [PluginEvent(ServerEventType.WarheadStart)]
    public bool WarheadStartHook(bool isAutomatic, IPlayer player, bool isResumed)
    {
        if (player != null) return true;
        var ev = new WarheadStartEvent(isResumed, isAutomatic);
        WarheadStart.RaiseSafely(ev);
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
        var synapseTarget = target.GetSynapsePlayer();
        var ragDoll = body.GetSynapseRagDoll();
        var ev = new Scp049ReviveEvent(synapse049, synapseTarget, ragDoll, true);
        Scp049Revive.RaiseSafely(ev);
        return ev.Allow;
    }

    [PluginEvent(ServerEventType.Scp049StartResurrectingBody)]
    public bool Scp049StartResurrectingBodyHook(IPlayer player, IPlayer target, BasicRagdoll body, bool canResurrect)
    {
        if (!canResurrect) return true;
        var synapse049 = player?.GetSynapsePlayer();
        var synapseTarget = target?.GetSynapsePlayer();
        var ragDoll = body?.GetSynapseRagDoll();
        if (synapse049 == null || synapseTarget == null || ragDoll == null) return true;
        var ev = new Scp049ReviveEvent(synapse049, synapseTarget, ragDoll, false);
        Scp049Revive.RaiseSafely(ev);
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
        var ev = new DisarmEvent(synapsePlayer.Inventory.ItemInHand, ItemInteractState.Finalize, synapsePlayer,
            synapseTarget);
        Disarm.RaiseSafely(ev);
        return ev.Allow;
    }

    [PluginEvent(ServerEventType.PlayerPreCoinFlip)]
    public PlayerPreCoinFlipCancellationData PlayerFlipCoinHook(IPlayer player)
    {
        var synapsePlayer = player.GetSynapsePlayer();
        var synapseItem = synapsePlayer.Inventory.ItemInHand;
        var ev = new FlipCoinEvent(synapseItem, synapsePlayer, Random.value >= 0.5f);
        FlipCoin.RaiseSafely(ev);

        return !ev.Allow
            ? PlayerPreCoinFlipCancellationData.PreventFlip()
            : PlayerPreCoinFlipCancellationData.Override(ev.Tails);
    }
}

public partial class MapEvents
{
    [PluginEvent(ServerEventType.WarheadDetonation)]
    public bool DetonateWarheadHook()
    {
        var ev = new DetonateWarheadEvent();
        DetonateWarhead.RaiseSafely(ev);
        return ev.Allow;
    }

    [PluginEvent(ServerEventType.WarheadStop)]
    public bool PlayerCancelWarhead(IPlayer player)
    {
        var synapsePlayer = player.GetSynapsePlayer();
        var ev = new CancelWarheadEvent(synapsePlayer, true);

        CancelWarhead.RaiseSafely(ev);

        return ev.Allow;
    }
}