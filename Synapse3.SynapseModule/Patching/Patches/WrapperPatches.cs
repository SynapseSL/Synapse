using System;
using HarmonyLib;
using Interactables.Interobjects;
using Mirror;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp096;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using UnityEngine;

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
[SynapsePatch("PlayerStaminaUse", PatchType.Wrapper)]
public static class PlayerStaminaUsePatch
{

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FpcStateProcessor), nameof(FpcStateProcessor.ServerUseRate), MethodType.Getter)]
    public static bool PlayerLoadComponent(FpcStateProcessor __instance, ref float __result)
    {
        
        var player = __instance._hub.GetSynapsePlayer();

        if (player.RoleManager.CurrentRole.ActiveTime <= __instance._respawnImmunity)
        {
            __result = 0;
            return false;
        }

        var staminaUse = player.StaminaUseRate == -1 ? __instance._useRate : player.StaminaUseRate;
        staminaUse *= player.Hub.inventory.StaminaUsageMultiplier;
        foreach (var effect in player.Hub.playerEffectsController.AllEffects)
        {
            if (effect is IStaminaModifier staminaModifier && staminaModifier.StaminaModifierActive)
            {
                staminaUse *= staminaModifier.StaminaUsageMultiplier;
            }
        }

        __result = staminaUse;
        return false;
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
    public static bool MaxAux(Scp079AuxManager __instance, ref float __result)
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
    public static bool MaxAux(Scp079AuxManager __instance, ref float __result)
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
    public static bool MaxAux(Scp096RageManager __instance, ref float __result)
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