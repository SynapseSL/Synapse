using System;
using System.Collections.ObjectModel;
using HarmonyLib;
using Hazards;
using Hints;
using Interactables.Interobjects;
using InventorySystem.Searching;
using Mirror;
using Neuron.Core.Dev;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp173;
using PlayerRoles.Voice;
using PluginAPI.Core;
using PluginAPI.Enums;
using RelativePositioning;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using VoiceChat;
using VoiceChat.Networking;
using static PlayerList;

namespace Synapse3.SynapseModule.Patching.Patches;

[Automatic]
[SynapsePatch("PlayerLoadComponent", PatchType.Wrapper)]
public static class PlayerLoadComponentPatch
{
    private static readonly DummyService DummyService;
    static PlayerLoadComponentPatch() => DummyService = Synapse.Get<DummyService>();
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.Awake))]
    public static void PlayerLoadComponent(ReferenceHub __instance)
    {
        try
        {
            var player = __instance.GetComponent<SynapsePlayer>();
            if (player == null)
            {
                if (ReferenceHub.AllHubs.Count == 0)
                {
                    player = __instance.gameObject.AddComponent<SynapseServerPlayer>();
                }
                else if (__instance.transform.parent == DummyService._dummyParent)
                {
                    __instance.transform.parent = null;
                    player = __instance.gameObject.AddComponent<DummyPlayer>();
                }
                else
                {
                    player = __instance.gameObject.AddComponent<SynapsePlayer>();
                }
            }
            
            var ev = new LoadComponentEvent(__instance.gameObject, player);
            Synapse.Get<PlayerEvents>().LoadComponent.RaiseSafely(ev);
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error($"S3 Events: LoadComponent Event Failed\n{ex}");
        }
    }
}

[Automatic]
[SynapsePatch("Scp079MaxAuxiliary", PatchType.Wrapper)]
public static class Scp079MaxAuxiliaryPatch
{

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079AuxManager), nameof(Scp079AuxManager.MaxAux), MethodType.Getter)]
    public static bool MaxAux(Scp079AuxManager __instance, ref float __result)
    {
        __result = __instance.Owner.GetSynapsePlayer().ScpController.Scp079.MaxEnergy;
        return false;
    }
}

[Automatic]
[SynapsePatch("Scp079RegenAuxiliary", PatchType.Wrapper)]
public static class Scp079RegenAuxiliaryPatch
{

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079AuxManager), nameof(Scp079AuxManager.RegenSpeed), MethodType.Getter)]
    public static bool RegenSpeed(Scp079AuxManager __instance, ref float __result)
    {
        __result = __instance.Owner.GetSynapsePlayer().ScpController.Scp079.RegenEnergy;
        return false;
    }
}

[Automatic]
[SynapsePatch("DynamicShieldRegen", PatchType.Wrapper)]
public static class Scp096RegenerationPatch
{

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DynamicHumeShieldController), nameof(DynamicHumeShieldController.HsRegeneration), MethodType.Getter)]
    public static bool HsRegeneration(Scp079AuxManager __instance, ref float __result)
    {
        var player = __instance.Owner.GetSynapsePlayer();
        switch (player.RoleType)
        {
            case RoleTypeId.Scp173:
                if (player.ScpController.Scp173._shieldRegeneration == -1)
                {
                    return true;
                }
                else
                {
                    __result = player.ScpController.Scp173._shieldRegeneration;
                    return false;
                }
            case RoleTypeId.Scp106:
                if (player.ScpController.Scp106._shieldRegeneration == -1)
                {
                    return true;
                }
                else
                {
                    __result = player.ScpController.Scp106._shieldRegeneration;
                    return false;
                }
            case RoleTypeId.Scp096:
                if (player.ScpController.Scp096._shieldRegeneration == -1)
                {
                    return true;
                }
                else
                {
                    __result = player.ScpController.Scp096._shieldRegeneration;
                    return false;
                }
            case RoleTypeId.Scp939:
                if (player.ScpController.Scp939._shieldRegeneration == -1)
                {
                    return true;
                }
                else
                {
                    __result = player.ScpController.Scp939._shieldRegeneration;
                    return false;
                }
        }
        return true;
    }
}


[Automatic]
[SynapsePatch("Scp096SheldMax", PatchType.Wrapper)]
public static class Scp096SheldMaxPatch
{

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DynamicHumeShieldController), nameof(DynamicHumeShieldController.HsMax), MethodType.Getter)]
    public static bool HsMax(Scp096RageManager __instance, ref float __result)
    {
        var player = __instance.Owner.GetSynapsePlayer();
        switch (player.RoleType)
        {
            case RoleTypeId.Scp173:
                if (player.ScpController.Scp173._maxShield == -1)
                {
                    return true;
                }
                else
                {
                    __result = player.ScpController.Scp173._maxShield;
                    return false;
                }
            case RoleTypeId.Scp106:
                if (player.ScpController.Scp106._maxShield == -1)
                {
                    return true;
                }
                else
                {
                    __result = player.ScpController.Scp106._maxShield;
                    return false;
                }
            case RoleTypeId.Scp096:
                if (player.ScpController.Scp096._maxShield == -1)
                {
                    return true;
                }
                else
                {
                    __result = player.ScpController.Scp096._maxShield;
                    return false;
                }
            case RoleTypeId.Scp939:
                if (player.ScpController.Scp939._maxShield == -1)
                {
                    return true;
                }
                else
                {
                    __result = player.ScpController.Scp939._maxShield;
                    return false;
                }
        }
        return true;
    }
}

[Automatic]
[SynapsePatch("RedirectRoleWrite", PatchType.Wrapper)]
public static class RedirectRoleWritePatch
{
    private static readonly PlayerService _playerService;
    static RedirectRoleWritePatch() => _playerService = Synapse.Get<PlayerService>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RoleSyncInfo), nameof(RoleSyncInfo.Write))]
    public static bool RedirectWrite(RoleSyncInfo __instance, NetworkWriter writer)
    {
        var receiver = _playerService.GetPlayer(__instance._receiverNetId);
        var target = _playerService.GetPlayer(__instance._targetNetId);
        if (target == null || receiver == null) return true;
        target.FakeRoleManager.WriteRoleSyncInfoFor(receiver, writer);
        return false;
    }
}

[Automatic]
[SynapsePatch("UnDestroyableDoor", PatchType.Wrapper)]
public static class UnDestroyableDoorPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BreakableDoor), nameof(BreakableDoor.ServerDamage))]
    public static bool OnDoorDamage(BreakableDoor __instance,float hp)
    {
        try
        {
            return !__instance.GetSynapseDoor()?.UnDestroyable ?? true;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Sy3 API: Damage Door failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp173BlinkCooldDown", PatchType.Wrapper)]
public static class Scp173BlinkCooldDownPatch
{

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173BlinkTimer), nameof(Scp173BlinkTimer.OnObserversChanged))]
    public static void OnObserversChanged(Scp173BlinkTimer __instance, int prev, int current)
    {
        var player = __instance.Role._lastOwner.GetSynapsePlayer();

        if (prev == 0 && __instance.RemainingSustainPercent == 0f)
        {
            __instance._initialStopTime = NetworkTime.time;
            __instance._totalCooldown = player.ScpController.Scp173.BlinkCooldownBase;
        }

        __instance._totalCooldown += player.ScpController.Scp173.BlinkCooldownPerPlayer * (current - prev);
        __instance._endSustainTime = ((current > 0) ? (-1.0) : (NetworkTime.time + 3.0));
        __instance.ServerSendRpc(toAll: true);
    }
}

[Automatic]
[SynapsePatch("Scp173ObserversList", PatchType.Wrapper)]
public static class Scp173ObserversListPatch
{

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.CheckRemovedPlayer))]
    public static bool CheckRemovedPlayer(Scp173ObserversTracker __instance, ReferenceHub ply)
    {
        var player = __instance.Owner.GetSynapsePlayer();
        var controler = player.ScpController.Scp173;

        if (__instance.Observers.Remove(ply) | controler.Observer.Remove(player))
        {
            __instance.CurrentObservers--;
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.UpdateObservers))]
    public static bool UpdateObserver(Scp173ObserversTracker __instance)
    {
        var observers = 0;

        foreach (ReferenceHub hub in ReferenceHub.AllHubs)
        {
            var player = hub.GetSynapsePlayer();
            var scp = __instance.Owner.GetSynapsePlayer();
            var controler = scp.ScpController.Scp173;

            if (scp == player || player.RoleType is RoleTypeId.Spectator or RoleTypeId.None) continue;

            if (__instance.IsObservedBy(hub, 0.2f))
            {
                observers += 1;
                __instance.Observers.Add(hub);
                controler.Observer.Add(scp);
            }
            else
            {
                controler.Observer.Remove(scp);
                __instance.Observers.Remove(hub);
            }
        }

        __instance.CurrentObservers = observers;

        if (!__instance.Owner.isLocalPlayer)
        {
            __instance.ServerSendRpc(toAll: true);
        }
        return false;
    }
}

/*
[Patches]
[HarmonyPatch]
internal static class WrapperPatches
{    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Lift), nameof(Lift.UseLift))]
    public static bool OnUseLift(Lift __instance, out bool __result)
    {
        __result = false;
        try
        {
            if (!__instance.operative || AlphaWarheadController.Host.timeToDetonation == 0f ||
                __instance._locked) return false;

            __instance.operative = false;
            __instance.GetSynapseElevator().MoveToNext();
            __result = true;
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 API: Use Lift failed\n" + ex);
            return true;
        }
    }
}
*/