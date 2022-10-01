using System;
using CustomPlayerEffects;
using HarmonyLib;
using Interactables.Interobjects;
using InventorySystem.Items;
using InventorySystem.Items.Armor;
using MEC;
using Mirror;
using Neuron.Core.Logging;
using PlayerStatsSystem;
using Respawning.NamingRules;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;
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
            if (hub == null) return false;
            var prefab = hub.characterClassManager.CurRole?.model_ragdoll;

            if (prefab == null || !Object.Instantiate(prefab).TryGetComponent<Ragdoll>(out var ragdoll))
                return false;

            var info = new RagdollInfo(hub, handler, prefab.transform.localPosition,
                prefab.transform.localRotation);
            
            ragdoll.Info = info;
            ragdoll.SetSyncVar(info, ref ragdoll.Info, 1uL); // I don't use NetworkInfo to not call the patch of the get

            NetworkServer.Spawn(ragdoll.gameObject);

            _ = new SynapseRagdoll(ragdoll, true);

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
    [HarmonyPatch(typeof(UnitNamingManager), nameof(UnitNamingManager.GenerateDefaults))]
    public static bool OnGenerateDefaults() => false;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FirstPersonController), nameof(FirstPersonController.GetSpeed))]
    public static bool GetSpeed(FirstPersonController __instance, out float speed)
    {
        try
        {
            speed = 0f;
            var player = __instance.GetSynapsePlayer();
            if (player == null) return false;
            __instance.curRole = player.ClassManager.Classes.SafeGet(player.RoleType);
            var isScp = player.ClassManager.IsAnyScp();

            if (!__instance.isLocalPlayer)
            {
                __instance.IsSneaking = player.MovementState == PlayerMovementState.Sneaking && !isScp;
            }

            if (!isScp)
            {
                speed = __instance.staminaController.AllowMaxSpeed
                    ? __instance.curRole.runSpeed
                    : __instance.curRole.walkSpeed;
                
                speed *= __instance.staminaController.AllowMaxSpeed ? player.SprintSpeed : player.WalkSpeed;
            }
            else
            {
                if (player.ScpsController.CurrentScp != null)
                {
                    speed = player.ScpsController.CurrentScp.MaxSpeed;
                }
                else if(player.RoleType == RoleType.Scp106 && __instance.Slowdown106)
                {
                    speed = 1f;
                }
            }

            if (__instance.IsSneaking)
            {
                speed += 0.4f;
            }

            foreach (var effect in player.PlayerEffectsController.AllEffects)
            {
                if (!effect.Value.IsEnabled || effect.Value is not IMovementSpeedEffect speedEffect) continue;
                speed = speedEffect.GetMovementSpeed(speed);
                if (speedEffect.DisableSprint)
                {
                    __instance.IsSprinting = false;
                }
            }

            if (player.VanillaInventory.TryGetBodyArmor(out var armor))
            {
                BodyArmorUtils.GetMovementProperties(player.Team, armor, out var factor, out _);
                speed += factor;
            }

            if (player.VanillaInventory.CurInstance is not IMobilityModifyingItem mobilityModifyingItem) return false;
            
            speed += mobilityModifyingItem.MovementSpeedMultiplier;
            if (speed > mobilityModifyingItem.MovementSpeedLimiter &&
                mobilityModifyingItem.MovementSpeedLimiter >= 0f)
                speed = mobilityModifyingItem.MovementSpeedLimiter;
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 API: GetSpeed failed\n" + ex);
            speed = 0f;
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.NetworkInfo), MethodType.Setter)]
    public static bool OnSetInfo(Ragdoll __instance, RagdollInfo value)
    {
        try
        {
            var ragodll = __instance.GetSynapseRagdoll();
            if (ragodll == null) return false;
            __instance.Info = value;
            ragodll.FakeInfoManger.UpdateAll();
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 API: SetRagdollInfo failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.NetworkCurClass), MethodType.Setter)]
    public static bool OnSetRole(CharacterClassManager __instance, RoleType value)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (player == null) return false;
            __instance.CurClass = value;
            player.FakeRoleManager.UpdateAll();

            //This is to check if any Conditions will now be true since the Player changed his Role
            foreach (var otherPlayer in Synapse.Get<PlayerService>().Players)
            {
                if (otherPlayer == player) continue;
                otherPlayer.FakeRoleManager.UpdatePlayer(player);
            }
            return true;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 API: SetRole failed\n" + ex);
            return true;
        }
    }
}