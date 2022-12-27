using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Items.MicroHID;
using Mirror;
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
using System.Reflection;
using UnityEngine;
using Utils.Networking;

namespace Synapse3.SynapseModule.Patching.Patches;

[Automatic]
[SynapsePatch("Scp049Attack", PatchType.ScpEvent)]
public static class Scp049AttackPatch
{
    static ScpEvents _scp;
    static FieldInfo _eventField;

    static Scp049AttackPatch()
    {
        _scp = Synapse.Get<ScpEvents>();
        _eventField = typeof(Scp049AttackAbility)
            .GetField(nameof(Scp049AttackAbility.OnServerHit), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp049AttackAbility), nameof(Scp049AttackAbility.ServerProcessCmd))]
    public static bool OnServerProcessCmd(Scp049AttackAbility __instance, NetworkReader reader)
    {
        try
        {
            if (!__instance.Cooldown.IsReady || __instance._resurrect.IsInProgress)
                return false;
            __instance._target = reader.ReadReferenceHub();
            if (__instance._target == null || !__instance.IsTargetValid(__instance._target))
                return false;

            var effect = __instance._target.playerEffectsController.GetEffect<CardiacArrest>();
            var instantKill = effect.IsEnabled;

            var player = __instance.GetSynapsePlayer();
            var victime = __instance._target.GetSynapsePlayer();
            var ev = new Scp049AttackEvent(player, victime,  instantKill ? -1 : 0, 1.5f, effect.IsEnabled, true);

            _scp.Scp049Attack.RaiseSafely(ev);

            if (!ev.Allow) return false;

            __instance.Cooldown.Trigger(ev.Cooldown);
            
            if (ev.Damage != 0)
            {
                __instance._target.playerStats.DealDamage(new Scp049DamageHandler(__instance.Owner, ev.Damage, Scp049DamageHandler.AttackType.Instakill));
            }
            if (!ev.CardiacArrestEffect)
            {
                effect.SetAttacker(__instance.Owner);
                effect.Intensity = (byte)1;
                effect.ServerChangeDuration(__instance._statusEffectDuration);
            }

            CallOnServerHit(__instance, __instance._target);
            __instance.ServerSendRpc(true);
            Hitmarker.SendHitmarker(__instance.Owner, 1f);
            return false;
        }
        catch (Exception e)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp049Attack failed\n" + e);
            return true;
        }
    }

    private static void CallOnServerHit(Scp049AttackAbility instance, ReferenceHub hub)
    {
        var eventHandlerList = _eventField.GetValue(instance);
        if (eventHandlerList == null) return;
        var event_invoke = eventHandlerList.GetType().GetMethod("Invoke");
        if (event_invoke == null) return;
        event_invoke.Invoke(eventHandlerList, new object[1] { hub });
    }
}

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
            var num = Physics.OverlapSphereNonAlloc(__instance.OverlapSphereOrigin, __instance._detectionRadius, ZombieAttackAbility.DetectionsNonAlloc, ZombieAttackAbility.DetectionMask);
            __instance._syncAttack = AttackResult.None;

            foreach (var gameObject in ZombieAttackAbility.DetectionsNonAlloc)
            {
                if (!gameObject.TryGetComponent<IDestructible>(out var component)
                    || Physics.Linecast(__instance.PlyCam.position, component.CenterOfMass, ZombieAttackAbility.BlockerMask)) 
                    continue;

                var hitboxIdentity = component as HitboxIdentity;
                var victime = hitboxIdentity?.TargetHub?.GetSynapsePlayer(); ;
                var isPlayer = victime != null;

                if (isPlayer && !ZombieAttackAbility.TargettedPlayers.Remove(hitboxIdentity.TargetHub)) 
                    continue;

                var damageHandler = (Scp049DamageHandler)__instance.DamageHandler;

                if (isPlayer)
                {
                    var scp = __instance.GetSynapsePlayer();
                    
                    var damage = __instance.DamageAmount;
                    var ev = new Scp0492AttackEvent(scp, victime, damage, true);

                    _scp.Scp0492Attack.RaiseSafely(ev);

                    if (!ev.Allow) continue;

                    damageHandler.Damage = ev.Damage;
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

                    var target = hitboxIdentity.TargetHub.GetSynapsePlayer();
                    var scp = __instance._scpRole.GetSynapsePlayer(); 
                    var flag = __instance._targetCounter.HasTarget(target);
                    var damage = flag ? __instance._humanTargetDamage : __instance._humanNontargetDamage;
                    var charge = __instance._damageType  == Scp096DamageHandler.AttackType.Charge;

                    var ev = new Scp096AttackEvent(scp, target, charge, damage);

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
        var eventFieldWindow = typeof(Scp096HitHandler)
            .GetField(nameof(Scp096HitHandler.OnWindowHit), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        var eventHandlerList = eventFieldWindow.GetValue(instance);
        if (eventHandlerList == null) return;
        var event_invoke = eventHandlerList.GetType().GetMethod("Invoke");
        if (event_invoke == null) return;
        event_invoke.Invoke(eventHandlerList, new object[1] { window });
    }

    private static void CallOnWindowHit(Scp096HitHandler instance, ReferenceHub hub)
    {
        var eventFieldPlayer = typeof(Scp096HitHandler)
            .GetField(nameof(Scp096HitHandler.OnPlayerHit), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        var eventHandlerList = eventFieldPlayer.GetValue(instance);
        if (eventHandlerList == null) return;
        var event_invoke = eventHandlerList.GetType().GetMethod("Invoke");
        if (event_invoke == null) return;
        event_invoke.Invoke(eventHandlerList, new object[1] { hub });
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
        var eventField = typeof(Scp106AttackPatch)
            .GetField(nameof(Scp106Attack.OnPlayerTeleported), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        NeuronLogger.For<Synapse>().Error($"Sy3 Event: Scp106Attack {eventField}");
        var eventHandlerList = eventField.GetValue(null);
        if (eventHandlerList == null) return; 
        var event_invoke = eventHandlerList.GetType().GetMethod("Invoke");
        if (event_invoke == null) return;
        event_invoke.Invoke(eventHandlerList, new object[1] { hub });
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