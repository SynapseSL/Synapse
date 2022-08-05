using System;
using HarmonyLib;
using Interactables.Interobjects;
using Mirror;
using Neuron.Core.Logging;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Patches;

[Patches]
internal static class WrapperPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.LoadComponents))]
    public static void LoadComponents(ReferenceHub __instance)
    {
        try
        {
            var dummy = Synapse.Get<DummyService>();
            var player = __instance.GetComponent<SynapsePlayer>();
            if (player == null)
            {
                //At this point nothing is initiated inside the GameObject therefore is this the only solution I found
                if (ReferenceHub.Hubs.Count == 0)
                {
                    player = __instance.gameObject.AddComponent<SynapseServerPlayer>();
                }
                else if (__instance.transform.parent == dummy._dummyParent)
                {
                    player = __instance.gameObject.AddComponent<DummyPlayer>();
                }
                else
                {
                    player = __instance.gameObject.AddComponent<SynapsePlayer>();
                }
            }
            
            var ev = new LoadComponentEvent(__instance.gameObject, player);
            Synapse.Get<PlayerEvents>().LoadComponent.Raise(ev);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error($"S3 Events: LoadComponent Event Failed\n{ex}");
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.ServerSpawnRagdoll))]
    public static bool SpawnRagDoll(ReferenceHub hub, DamageHandlerBase handler)
    {
        try
        {
            if (hub is null) return false;

            var prefab = hub.characterClassManager.CurRole?.model_ragdoll;

            if (prefab == null || !UnityEngine.Object.Instantiate(prefab).TryGetComponent<Ragdoll>(out var ragdoll))
                return false;

            ragdoll.NetworkInfo = new RagdollInfo(hub, handler, prefab.transform.localPosition,
                prefab.transform.localRotation);
            
            NetworkServer.Spawn(ragdoll.gameObject);

            _ = new SynapseRagdoll(ragdoll);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error($"S3 Objects: Spawn Ragdoll Failed\n{ex}");
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Lift), nameof(Lift.UseLift))]
    public static bool OnUseLift(Lift __instance, out bool __result)
    {
        __result = false;
        try
        {
            if (!__instance.operative || AlphaWarheadController.Host.timeToDetonation == 0f ||
                __instance._locked) return false;

            __instance.GetSynapseElevator().MoveToNext();
            __instance.operative = false;
            __result = true;
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 API: Use Lift failed\n" + ex);
            return true;
        }
    }
    
    //So the Client does only send the Escape Command the first time he is at the exit as Scientiest/D-Personnel so other Roles cant escape or if you replace a D-Personnel/Scientist and try to escape a second time.
    //In order to fix this we block this Command entirely and do everything on our own.
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.UserCode_CmdRegisterEscape))]
    [HarmonyPrefix]
    public static bool RegisterEscape() => false;

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
            NeuronLogger.For<Synapse>().Error("Sy3 API: Damage Door failed\n" + ex);
            return true;
        }
    }
}