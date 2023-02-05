using System;
using HarmonyLib;
using Interactables.Interobjects;
using Mirror;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp173;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Player.ScpController;

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
#if !PATCHLESS

[Automatic]
[SynapsePatch("Scp079MaxAuxiliary", PatchType.Wrapper)]
public static class Scp079MaxAuxiliaryPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079AuxManager), nameof(Scp079AuxManager.MaxAux), MethodType.Getter)]
    public static bool MaxAux(Scp079AuxManager __instance, ref float __result)
    {
        try
        {
            __result = __instance.Owner.GetSynapsePlayer().MainScpController.Scp079.MaxEnergy;
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Scp079 Max Auxiliary Patch failed\n" + ex);
            return true;
        }
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
        try
        {
            __result = __instance.Owner.GetSynapsePlayer().MainScpController.Scp079.RegenEnergy;
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Scp079 Regen Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("DynamicShieldRegen", PatchType.Wrapper)]
public static class Scp096RegenerationPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DynamicHumeShieldController), nameof(DynamicHumeShieldController.HsRegeneration),
        MethodType.Getter)]
    public static bool HsRegeneration(Scp079AuxManager __instance, ref float __result)
    {
        try
        {
            var player = __instance.Owner.GetSynapsePlayer();
            if (player.MainScpController.CurrentController is not IScpShieldController shieldController) return true;

            if (shieldController.UseDefaultShieldRegeneration) return true;
            else
            {
                __result = shieldController.ShieldRegeneration;
                return false;
            }
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Dynamic Shield Regeneration Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp096ShieldMax", PatchType.Wrapper)]
public static class Scp096ShieldMaxPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DynamicHumeShieldController), nameof(DynamicHumeShieldController.HsMax), MethodType.Getter)]
    public static bool HsMax(Scp096RageManager __instance, ref float __result)
    {
        try
        {
            var player = __instance.Owner.GetSynapsePlayer();
            if (player.MainScpController.CurrentController is not IScpShieldController shieldController) return true;

            if (shieldController.UseDefaultMaxShield) return true;
            else
            {
                __result = shieldController.MaxShield;
                return false;
            }
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Scp096 Shield Max Patch failed\n" + ex);
            return true;
        }
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
        try
        {
            var receiver = _playerService.GetPlayer(__instance._receiverNetId);
            var target = _playerService.GetPlayer(__instance._targetNetId);
            if (target == null || receiver == null) return true;
            target.FakeRoleManager.WriteRoleSyncInfoFor(receiver, writer);
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Redirect Rolewrite Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("UnDestroyableDoor", PatchType.Wrapper)]
public static class UnDestroyableDoorPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BreakableDoor), nameof(BreakableDoor.ServerDamage))]
    public static bool OnDoorDamage(BreakableDoor __instance, float hp)
    {
        try
        {
            return !__instance.GetSynapseDoor()?.UnDestroyable ?? true;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Undestroyable Door Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp173BlinkCooldDown", PatchType.Wrapper)]
public static class Scp173BlinkCoolDownPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173BlinkTimer), nameof(Scp173BlinkTimer.OnObserversChanged))]
    public static void OnObserversChanged(Scp173BlinkTimer __instance, int prev, int current)
    {
        try
        {
            var player = __instance.Role._lastOwner.GetSynapsePlayer();

            if (prev == 0 && __instance.RemainingSustainPercent == 0f)
            {
                __instance._initialStopTime = NetworkTime.time;
                __instance._totalCooldown = player.MainScpController.Scp173.BlinkCooldownBase;
            }

            __instance._totalCooldown += player.MainScpController.Scp173.BlinkCooldownPerPlayer * (current - prev);
            __instance._endSustainTime = ((current > 0) ? (-1.0) : (NetworkTime.time + 3.0));
            __instance.ServerSendRpc(true);
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Scp173 Blink Cooldown Patch failed\n" + ex);
        }
    }
}
#endif