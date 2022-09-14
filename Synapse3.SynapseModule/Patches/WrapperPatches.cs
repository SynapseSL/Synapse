using System;
using HarmonyLib;
using Interactables.Interobjects;
using MEC;
using Mirror;
using Neuron.Core.Logging;
using PlayerStatsSystem;
using Respawning.NamingRules;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Player;
using Object = UnityEngine.Object;

namespace Synapse3.SynapseModule.Patches;

[Patches]
[HarmonyPatch]
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

            if (prefab == null || !Object.Instantiate(prefab).TryGetComponent<Ragdoll>(out var ragdoll))
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SerializeSyncVars))]
    public static bool OnSerializeClassManager(CharacterClassManager __instance, out  bool __result, NetworkWriter writer,
        bool forceAll)
    {
        __result = false;
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (player == null || player.VisibleRole == RoleType.None) return true;
            if (!forceAll && (__instance.syncVarDirtyBits & 8ul) == 0ul) return true;

            if (forceAll)
            {
                writer.WriteString(__instance.Pastebin);
                writer.WriteBoolean(__instance.IntercomMuted);
                writer.WriteBoolean(__instance.NoclipEnabled);
                writer.WriteSByte((sbyte)player.VisibleRole);
                writer.WriteByte(__instance.CurSpawnableTeamType);
                writer.WriteString(__instance.CurUnitName);
                writer.WriteBoolean(__instance.RoundStarted);
                writer.WriteBoolean(__instance.IsVerified);
                writer.WriteString(__instance.SyncedUserId);
                __result = true;
                return false;
            }

            writer.WriteUInt64(__instance.syncVarDirtyBits);
            
            if ((__instance.syncVarDirtyBits & 1UL) != 0UL)
            {
                writer.WriteString(__instance.Pastebin);
                __result = true;
            }
            if ((__instance.syncVarDirtyBits & 2UL) != 0UL)
            {
                writer.WriteBoolean(__instance.IntercomMuted);
                __result = true;
            }
            if ((__instance.syncVarDirtyBits & 4UL) != 0UL)
            {
                writer.WriteBoolean(__instance.NoclipEnabled);
                __result = true;
            }
            if ((__instance.syncVarDirtyBits & 8UL) != 0UL)
            {
                GeneratedNetworkCode._Write_RoleType(writer, player.VisibleRole);
                __result = true;
            }
            if ((__instance.syncVarDirtyBits & 16UL) != 0UL)
            {
                writer.WriteByte(__instance.CurSpawnableTeamType);
                __result = true;
            }
            if ((__instance.syncVarDirtyBits & 32UL) != 0UL)
            {
                writer.WriteString(__instance.CurUnitName);
                __result = true;
            }
            if ((__instance.syncVarDirtyBits & 64UL) != 0UL)
            {
                writer.WriteBoolean(__instance.RoundStarted);
                __result = true;
            }
            if ((__instance.syncVarDirtyBits & 128UL) != 0UL)
            {
                writer.WriteBoolean(__instance.IsVerified);
                __result = true;
            }
            if ((__instance.syncVarDirtyBits & 256UL) != 0UL)
            {
                writer.WriteString(__instance.SyncedUserId);
                __result = true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 API: Serialize ClassManager failed\n" + ex);
            return true;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetClassIDAdv))]
    public static void RefreshVisibleRole(CharacterClassManager __instance)
    {
        var player = __instance.GetSynapsePlayer();
        if (player.VisibleRole != RoleType.None)
            Timing.CallDelayed(0.2f, () => player.ChangeOneOwnVisibleRole(player.RoleType));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UnitNamingManager), nameof(UnitNamingManager.GenerateDefaults))]
    public static bool OnGenerateDefaults() => false;
}