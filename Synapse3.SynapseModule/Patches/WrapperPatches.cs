using System;
using HarmonyLib;
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

            new LoadComponentEvent(player).Raise();
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
}