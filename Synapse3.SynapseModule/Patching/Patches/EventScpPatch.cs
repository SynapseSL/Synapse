using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Items.MicroHID;
using Mirror;
using Mono.Cecil;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.PlayableScps.Scp049.Zombies;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.PlayableScps.Scp173;
using PlayerRoles.PlayableScps.Scp939;
using PlayerStatsSystem;
using PluginAPI.Enums;
using PluginAPI.Events;
using RelativePositioning;
using Synapse3.SynapseModule.Events;
using System;
using System.Diagnostics.Tracing;
using System.Reflection;
using UnityEngine;
using Utils.Networking;

namespace Synapse3.SynapseModule.Patching.Patches;

//TODO: FIX it, error null reference and maby the event are not call, see the reflexion
[Automatic]
[SynapsePatch("Scp0492Attack", PatchType.ScpEvent)]
public static class Scp0492AttackPatch
{
    static ScpEvents _scp;
    
    static Scp0492AttackPatch()
    {
        _scp = Synapse.Get<ScpEvents>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScpAttackAbilityBase<ZombieRole>), nameof(ScpAttackAbilityBase<ZombieRole>.ServerPerformAttack))]
    public static bool OnServerPerformAttack(ZombieAttackAbility __instance)
    {
        try
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: OnServerPerformAttack 1");
            var num = Physics.OverlapSphereNonAlloc(__instance.OverlapSphereOrigin, __instance._detectionRadius, ZombieAttackAbility.DetectionsNonAlloc, ZombieAttackAbility.DetectionMask);
            NeuronLogger.For<Synapse>().Error("Sy3 Event: OnServerPerformAttack 2"); 
            __instance._syncAttack = AttackResult.None;
            NeuronLogger.For<Synapse>().Error("Sy3 Event: OnServerPerformAttack 3");
            foreach (var gameObject in ZombieAttackAbility.DetectionsNonAlloc)
            {
                NeuronLogger.For<Synapse>().Error("Sy3 Event: OnServerPerformAttack other"); 
                if (!gameObject.TryGetComponent<IDestructible>(out var component)
                    || Physics.Linecast(__instance.PlyCam.position, component.CenterOfMass, ZombieAttackAbility.BlockerMask)) 
                    continue;
                NeuronLogger.For<Synapse>().Error("Sy3 Event: OnServerPerformAttack other 2");

                var hitboxIdentity = component as HitboxIdentity;
                var victime = hitboxIdentity?.TargetHub.GetSynapsePlayer();
                var isPlayer = victime != null;
                NeuronLogger.For<Synapse>().Error("Sy3 Event: OnServerPerformAttack other 3");

                if (isPlayer && !ZombieAttackAbility.TargettedPlayers.Remove(hitboxIdentity.TargetHub)) 
                    continue;
                NeuronLogger.For<Synapse>().Error("Sy3 Event: OnServerPerformAttack other 4");

                var damageHandler = (Scp049DamageHandler)__instance.DamageHandler;
                NeuronLogger.For<Synapse>().Error($"Sy3 Event: OnServerPerformAttack {isPlayer}");

                if (isPlayer)
                {
                    NeuronLogger.For<Synapse>().Error($"Sy3 Event: OnServerPerformAttack Raise");
                    var scp = __instance.GetSynapsePlayer();
                    
                    var damage = __instance.DamageAmount;
                    var ev = new Scp0492AttackEvent(scp, victime, damage, true);

                    _scp.Scp0492Attack.RaiseSafely(ev);

                    if (!ev.Allow) continue;

                    damageHandler.Damage = ev.Damage;
                    NeuronLogger.For<Synapse>().Error($"Sy3 Event: OnServerPerformAttack Raised");
                }

                if (!component.Damage(damageHandler.Damage, damageHandler, component.CenterOfMass))
                    continue;

                __instance.OnDestructibleDamaged(component);
                __instance._syncAttack |= AttackResult.AttackedObject;
                if (isPlayer)
                {
                    __instance._syncAttack |= AttackResult.AttackedHuman;
                    if (victime.Health <= 0f)
                    {
                        __instance._syncAttack |= AttackResult.KilledHuman;
                    }
                }
            }


            __instance.ServerSendRpc(toAll: true);
            return false;
        }
        catch (Exception e)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp0492Attack failed\n" + e);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp096Attack", PatchType.ScpEvent)]
public static class Scp096AttackPatch
{
    static ScpEvents _scp;

    static Scp096AttackPatch()
    {
        _scp = Synapse.Get<ScpEvents>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp096HitHandler), nameof(Scp096HitHandler.ProcessHits))]
    public static bool ServerProcessCmd(Scp096HitHandler __instance, ref Scp096HitResult __result, int count)
    {
        try
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp096AttackPatch");

            var scp096HitResult = Scp096HitResult.None;
            for (int i = 0; i < count; i++)
            {
                var collider = Scp096HitHandler.Hits[i];
                __instance.CheckDoorHit(collider);
                if (!collider.TryGetComponent<IDestructible>(out var component))
                    continue;

                var layerMask = Scp096HitHandler.SolidObjectMask & ~(1 << collider.gameObject.layer);
                if (Physics.Linecast(__instance._scpRole.CameraPosition, component.CenterOfMass, layerMask) 
                    || !__instance._hitNetIDs.Add(component.NetworkId))
                    continue;

                if (component is BreakableWindow breakableWindow)
                {
                    if (__instance.DealDamage(breakableWindow, __instance._windowDamage))
                    {
                        scp096HitResult |= Scp096HitResult.Window;
                        CallOnPlayerHit(__instance, breakableWindow);
                    }
                }
                else
                {
                    if (component is not HitboxIdentity hitboxIdentity || !__instance.IsHumanHitbox(hitboxIdentity))
                        continue;

                    NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp096AttackPatch raise");
                    var target = hitboxIdentity.TargetHub?.GetSynapsePlayer();
                    NeuronLogger.For<Synapse>().Error(__instance._scpRole == null);
                    var scp = __instance._scpRole.GetSynapsePlayer(); 
                    var flag = __instance._targetCounter.HasTarget(target);
                    var damage = flag ? __instance._humanTargetDamage : __instance._humanNontargetDamage;
                    var charge = __instance._damageType  == Scp096DamageHandler.AttackType.Charge;

                    var ev = new Scp096AttackEvent(scp, target, charge, damage);
                    NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp096AttackPatch raised");

                    _scp.Scp096Attack.RaiseSafely(ev);

                    if (!ev.Allow) continue;

                    if (__instance.DealDamage(hitboxIdentity, ev.Damage))
                    {
                        scp096HitResult |= Scp096HitResult.Human;
                        CallOnWindowHit(__instance, target);
                        if (!target.Hub.IsAlive())
                        {
                            scp096HitResult |= Scp096HitResult.Lethal;
                        }
                    }
                }
            }

            __instance.HitResult |= scp096HitResult;
            __result = scp096HitResult;
            return false;
        }
        catch (Exception e)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp096Attack failed\n" + e);
            return true;
        }
    }

    private static void CallOnPlayerHit(Scp096HitHandler instance, BreakableWindow window)
    {
        var eventDelegate = (MulticastDelegate)typeof(Scp096HitHandler)
            .GetField(nameof(Scp096HitHandler.OnWindowHit), BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(instance);
        if (eventDelegate != null)
        {
            foreach (var handler in eventDelegate.GetInvocationList())
            {
                handler.Method.Invoke(handler.Target, new object[] { instance, window });
            }
        }
    }

    private static void CallOnWindowHit(Scp096HitHandler instance, ReferenceHub hub)
    {
        var eventDelegate = (MulticastDelegate)typeof(Scp096HitHandler)
            .GetField(nameof(Scp096HitHandler.OnPlayerHit), BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(instance);
        if (eventDelegate != null)
        {
            foreach (var handler in eventDelegate.GetInvocationList())
            {
                handler.Method.Invoke(handler.Target, new object[] { instance, hub });
            }
        }
    }
}

[Automatic]
[SynapsePatch("Scp106Attack", PatchType.ScpEvent)]
public static class Scp106AttackPatch
{
    static ScpEvents _scp;

    static Scp106AttackPatch()
    {
        _scp = Synapse.Get<ScpEvents>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp106Attack), nameof(Scp106Attack.ServerShoot))]
    public static bool OnServerShoot(Scp106Attack __instance)
    {
        try
        {
            Scp106AttackEvent ev;

            using (new FpcBacktracker(__instance._targetHub, __instance._targetPosition, 0.35f))
            {
                var vector = __instance._targetPosition - __instance._ownerPosition;
                float sqrMagnitude = vector.sqrMagnitude;
                if (sqrMagnitude > __instance._maxRangeSqr)
                {
                    return false;
                }

                var forward = __instance.OwnerCam.forward;
                forward.y = 0f;
                vector.y = 0f;
                if (Physics.Linecast(__instance._ownerPosition, __instance._targetPosition, MicroHIDItem.WallMask))
                {
                    return false;
                }

                if (__instance._dotOverDistance.Evaluate(sqrMagnitude) > Vector3.Dot(vector.normalized, forward.normalized))
                {
                    __instance.SendCooldown(__instance._missCooldown);
                    return false;
                }

                var player = __instance.Owner.GetSynapsePlayer();
                var victime =__instance._targetHub.GetSynapsePlayer();

                ev = new Scp106AttackEvent(player, victime, __instance._damage, true, true);
                _scp.Scp106Attack.RaiseSafely(ev);

                if (!ev.Allow) return false;

                if (!EventManager.ExecuteEvent(ServerEventType.Scp106TeleportPlayer, __instance.Owner, __instance._targetHub))
                {
                    return false;
                }

                DamageHandlerBase handler = new ScpDamageHandler(__instance.Owner, __instance._damage, DeathTranslations.PocketDecay);
                if (!__instance._targetHub.playerStats.DealDamage(handler))
                {
                    return false;
                }

                player.ScpController.Scp106.PlayersInPocket.Add(victime);
            }

            __instance.SendCooldown(__instance._hitCooldown);
            __instance.Vigor.VigorAmount += 0.3f;
            __instance.ReduceSinkholeCooldown();
            Hitmarker.SendHitmarker(__instance.Owner, 1f);
            CallOnPlayerTeleported(__instance._targetHub);
            PlayerEffectsController playerEffectsController = __instance._targetHub.playerEffectsController;
            playerEffectsController.EnableEffect<Traumatized>(180f);
            if (ev.TakeToPocket)
                playerEffectsController.EnableEffect<Corroding>();

            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp106Attack failed\n" + ex);
            return true;
        }
    }

    private static void CallOnPlayerTeleported(ReferenceHub hub)
    {
        var eventDelegate = (MulticastDelegate)typeof(Scp106AttackPatch)
            .GetField(nameof(Scp106Attack.OnPlayerTeleported), BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(null);
        if (eventDelegate != null)
        {
            foreach (var handler in eventDelegate.GetInvocationList())
            {
                handler.Method.Invoke(handler.Target, new object[] { null, hub });
            }
        }
    }
}

[Automatic]
[SynapsePatch("Scp173Attack", PatchType.ScpEvent)]
public static class Scp173AttackPatch
{
    static ScpEvents _scp;

    static Scp173AttackPatch()
    {
        _scp = Synapse.Get<ScpEvents>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173SnapAbility), nameof(Scp173SnapAbility.ServerProcessCmd))]
    public static bool OnServerProcessCmd(Scp173SnapAbility __instance, NetworkReader reader)
    {
        try
        {
            __instance._targetHub = ReferenceHubReaderWriter.ReadReferenceHub(reader);
            if (__instance._observersTracker.IsObserved 
                || __instance._targetHub == null
                || __instance._targetHub.roleManager.CurrentRole is not IFpcRole fpcRole
                || __instance.IsSpeeding 
                || !EventManager.ExecuteEvent(ServerEventType.Scp173SnapPlayer, __instance.Owner, __instance._targetHub))
            {
                return false;
            }

            var fpcModule = __instance.ScpRole.FpcModule;
            var fpcModule2 = fpcRole.FpcModule;
            var playerCameraReference = __instance.Owner.PlayerCameraReference;
            var position = fpcModule2.Position;
            var position2 = fpcModule.Position;
            var rotation = playerCameraReference.rotation;
            fpcModule2.Position = fpcModule2.Tracer.GenerateBounds(0.4f, ignoreTeleports: true).ClosestPoint(RelativePositionSerialization.ReadRelativePosition(reader).Position);
            var bounds = fpcModule.Tracer.GenerateBounds(0.1f, ignoreTeleports: true);
            bounds.Encapsulate(fpcModule.Position + fpcModule.Motor.Velocity * 0.2f);
            fpcModule.Position = bounds.ClosestPoint(RelativePositionSerialization.ReadRelativePosition(reader).Position);
            playerCameraReference.rotation = LowPrecisionQuaternionSerializer.ReadLowPrecisionQuaternion(reader).Value;

            if (Scp173SnapAbility.TryHitTarget(playerCameraReference, out var target))
                goto changePose;

            var scp = __instance.GetSynapsePlayer();
            var victime = target.GetSynapsePlayer();
            var damageHandler = __instance.ScpRole.DamageHandler;
            var damamge = damageHandler.Damage;
            var ev = new Scp173AttackEvent(scp, victime, damamge, true);

            _scp.Scp173Attack.RaiseSafely(ev);

            if (!ev.Allow) goto changePose;

            __instance.ScpRole.DamageHandler.Damage = ev.Damage;

            if (target.playerStats.DealDamage(__instance.ScpRole.DamageHandler))
            {
                Hitmarker.SendHitmarker(__instance.Owner, 1f);
                if (__instance.ScpRole.SubroutineModule.TryGetSubroutine<Scp173AudioPlayer>(out var subroutine))
                {
                    subroutine.ServerSendSound(Scp173AudioPlayer.Scp173SoundId.Snap);
                }
            }

            changePose:
            fpcModule2.Position = position;
            fpcModule.Position = position2;
            playerCameraReference.rotation = rotation;

            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp173Attack failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp939Lunge", PatchType.ScpEvent)]
public static class Scp939LunchPatch
{
    static ScpEvents _scp;

    static Scp939LunchPatch()
    {
        _scp = Synapse.Get<ScpEvents>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp939DamageHandler), nameof(Scp939DamageHandler.ProcessDamage))]
    public static void OnServerProcessCmd(Scp939DamageHandler __instance, ReferenceHub ply)
    {
        try
        {
            var scp = __instance.Attacker.GetSynapsePlayer();
            var victime = ply.GetSynapsePlayer();
            var ev =  new Scp939AttackEvent(scp, victime, __instance.Damage, __instance._damageType);

            _scp.Scp939Attack.RaiseSafely(ev);

            if (!ev.Allow) return;

            __instance.Damage = ev.Damage;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: 939Attack failed\n" + ex);
        }
    }
}