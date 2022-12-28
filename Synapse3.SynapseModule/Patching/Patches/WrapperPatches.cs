using System;
using HarmonyLib;
using Interactables.Interobjects;
using Mirror;
using Neuron.Core.Meta;
using PlayerRoles;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;

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