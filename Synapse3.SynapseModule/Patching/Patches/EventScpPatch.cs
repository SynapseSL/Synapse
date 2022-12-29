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
using Synapse3.SynapseModule.Player;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Utils.Networking;
using static PlayerRoles.PlayableScps.Scp173.Scp173TeleportAbility;

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
            List<SynapsePlayer> ParsedPlayer = new List<SynapsePlayer>();

            var num = Physics.OverlapSphereNonAlloc(__instance.OverlapSphereOrigin, __instance._detectionRadius, ZombieAttackAbility.DetectionsNonAlloc, ZombieAttackAbility.DetectionMask);
            __instance._syncAttack = AttackResult.None;
            for (int i = 0; i < num; i++)
            {
                var gameObject = ZombieAttackAbility.DetectionsNonAlloc[i];
                if (!gameObject.TryGetComponent<IDestructible>(out var component)
                    || Physics.Linecast(__instance.PlyCam.position, component.CenterOfMass, ZombieAttackAbility.BlockerMask)) 
                    continue;

                var hitboxIdentity = component as HitboxIdentity;
                var victime = hitboxIdentity?.TargetHub.GetSynapsePlayer();
                var isPlayer = victime != null;

                if (!isPlayer || ParsedPlayer.Contains(victime)) continue;

                ParsedPlayer.Add(victime);

                var damageHandler = __instance.DamageHandler as AttackerDamageHandler;

                var scp = __instance.Owner.GetSynapsePlayer();
                    
                var damage = __instance.DamageAmount;
                if (scp.RoleType == RoleTypeId.Scp939)
                {
                    var ev = new Scp939AttackEvent(scp, victime, damage, Scp939DamageType.Claw);

                    _scp.Scp939Attack.RaiseSafely(ev);

                    damage = ev.Damage;
                }
                else
                {
                    var ev = new Scp0492AttackEvent(scp, victime, damage, true);

                    _scp.Scp0492Attack.RaiseSafely(ev);

                    if (!ev.Allow) continue;

                    damage = ev.Damage;
                }


                damageHandler.Damage = damage;

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
    public static bool ProcessHits(Scp096HitHandler __instance, ref Scp096HitResult __result, int count)
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

                int layerMask = (int)Scp096HitHandler.SolidObjectMask & ~(1 << collider.gameObject.layer);
                if (Physics.Linecast(__instance._scpRole.CameraPosition, component.CenterOfMass, layerMask) || !__instance._hitNetIDs.Add(component.NetworkId))
                {
                    continue;
                }

                if (component is BreakableWindow breakableWindow)
                {
                    if (__instance.DealDamage(breakableWindow, __instance._windowDamage))
                    {
                        scp096HitResult |= Scp096HitResult.Window;

                        Synapse3Extensions.RaiseEvent(__instance, nameof(Scp096HitHandler.OnWindowHit), breakableWindow);
                    }
                }
                else
                {
                    if (component is not HitboxIdentity hitboxIdentity || !__instance.IsHumanHitbox(hitboxIdentity))
                        continue;

                    var target = hitboxIdentity.TargetHub?.GetSynapsePlayer();
                    var scp = __instance._scpRole._lastOwner.GetSynapsePlayer(); 
                    var isTarget = __instance._targetCounter.HasTarget(target);
                    var damage = isTarget ? __instance._humanTargetDamage : __instance._humanNontargetDamage;
                    var charge = __instance._damageType  == Scp096DamageHandler.AttackType.Charge;

                    var ev = new Scp096AttackEvent(scp, target, charge, damage);

                    _scp.Scp096Attack.RaiseSafely(ev);

                    if (!ev.Allow) continue;

                    if (__instance.DealDamage(hitboxIdentity, ev.Damage))
                    {
                        scp096HitResult |= Scp096HitResult.Human;
                        Synapse3Extensions.RaiseEvent(__instance, nameof(Scp096HitHandler.OnPlayerHit), target.Hub);
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
}


[Automatic]
[SynapsePatch("Scp049Attack", PatchType.ScpEvent)]
public static class Scp049AttackPatch
{
    static ScpEvents _scp;

    static Scp049AttackPatch()
    {
        _scp = Synapse.Get<ScpEvents>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp049AttackAbility), nameof(Scp049AttackAbility.ServerProcessCmd))]
    public static bool ServerProcessCmd(Scp049AttackAbility __instance,NetworkReader reader)
    {
        try
        {
            if (!__instance.Cooldown.IsReady || __instance._resurrect.IsInProgress)
            {
                return false;
            }

            __instance._target = ReferenceHubReaderWriter.ReadReferenceHub(reader);
            if (__instance._target != null && __instance.IsTargetValid(__instance._target))
            {
                CardiacArrest effect = __instance._target.playerEffectsController.GetEffect<CardiacArrest>();

                var scp = __instance.Owner.GetSynapsePlayer();
                var victime = __instance._target.GetSynapsePlayer();
                var damage = effect.IsEnabled ? -1 : 0;
                var ev = new Scp049AttackEvent(scp, victime, damage, 1.5f, !effect.IsEnabled);

                __instance.Cooldown.Trigger(ev.Cooldown);
                if (ev.Damage != 0)
                {
                    __instance._target.playerStats.DealDamage(new Scp049DamageHandler(__instance.Owner, ev.Damage, Scp049DamageHandler.AttackType.Instakill));
                }
                
                if (ev.CardiacArrestEffect)
                {
                    effect.SetAttacker(__instance.Owner);
                    effect.Intensity = 1;
                    effect.ServerChangeDuration(__instance._statusEffectDuration);
                }

                Synapse3Extensions.RaiseEvent(__instance, nameof(Scp049AttackAbility.OnServerHit), __instance._target);
                __instance.ServerSendRpc(toAll: true);
                Hitmarker.SendHitmarker(__instance.Owner, 1f);
            }
            return false;
        }
        catch (Exception e)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp049Attack failed\n" + e);
            return true;
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
            Synapse3Extensions.RaiseEvent(typeof(Scp106Attack), nameof(Scp106Attack.OnPlayerTeleported), __instance._targetHub);
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
}

[Automatic]
[SynapsePatch("Scp173AttackSnap", PatchType.ScpEvent)]
public static class Scp173AttackSnapPatch
{
    static ScpEvents _scp;

    static Scp173AttackSnapPatch()
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
                || __instance._targetHub.roleManager.CurrentRole is not IFpcRole currentRole 
                || __instance.IsSpeeding)
            {
                return false;
            }

            var fpcModule = __instance.ScpRole.FpcModule;
            var fpcModule2 = currentRole.FpcModule;
            var playerCameraReference = __instance.Owner.PlayerCameraReference;
            var position = fpcModule2.Position;
            var position2 = fpcModule.Position;
            var rotation = playerCameraReference.rotation;
            fpcModule2.Position = fpcModule2.Tracer.GenerateBounds(0.4f, ignoreTeleports: true).ClosestPoint(RelativePositionSerialization.ReadRelativePosition(reader).Position);
            var bounds = fpcModule.Tracer.GenerateBounds(0.1f, ignoreTeleports: true);
            bounds.Encapsulate(fpcModule.Position + fpcModule.Motor.Velocity * 0.2f);
            fpcModule.Position = bounds.ClosestPoint(RelativePositionSerialization.ReadRelativePosition(reader).Position);
            playerCameraReference.rotation = LowPrecisionQuaternionSerializer.ReadLowPrecisionQuaternion(reader).Value;

            if (!Scp173SnapAbility.TryHitTarget(playerCameraReference, out var target))
                goto changePose;

            var scp = __instance.Owner.GetSynapsePlayer();
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
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp173AttackSnap failed\n" + ex);
            return true;
        }
    }
}


[Automatic]
[SynapsePatch("Scp173AttackTp", PatchType.ScpEvent)]
public static class Scp173AttackTpPatch
{
    static ScpEvents _scp;

    static Scp173AttackTpPatch()
    {
        _scp = Synapse.Get<ScpEvents>();
    }

    //I Can't patch, i just copy the method whith you edit and the server crash
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173TeleportAbility), nameof(Scp173TeleportAbility.ServerProcessCmd))]
    public static bool OnServerProcessCmd(Scp173TeleportAbility __instance, NetworkReader reader)
    {
        try
        {
            __instance._cmdData = (CmdTeleportData)reader.ReadByte();
            if (!__instance.HasDataFlag(CmdTeleportData.WantsToTeleport))
            {
                __instance.ServerSendRpc(toAll: true);
            }
            else
            {
                if (!__instance._blinkTimer.AbilityReady)
                {
                    return false;
                }

                Transform playerCameraReference = __instance.Owner.PlayerCameraReference;
                HashSet<ReferenceHub> prevObservers = new HashSet<ReferenceHub>(__instance._observersTracker.Observers);
                CmdTeleportData cmdData = __instance._cmdData;
                __instance._cmdData = (CmdTeleportData)0;
                __instance.ServerSendRpc(toAll: true);
                __instance._cmdData = cmdData;
                Quaternion rotation = playerCameraReference.rotation;
                playerCameraReference.rotation = reader.ReadQuaternion();
                bool num = __instance.TryBlink(reader.ReadSingle());
                playerCameraReference.rotation = rotation;
                if (!num)
                {
                    return false;
                }

                prevObservers.UnionWith(__instance._observersTracker.Observers);
                __instance.ServerSendRpc((ReferenceHub x) => prevObservers.Contains(x));
                __instance._audioSubroutine.ServerSendSound(Scp173AudioPlayer.Scp173SoundId.Teleport);
                if (__instance._breakneckSpeedsAbility.IsActive)
                {
                    return false;
                }

                int num2 = Physics.OverlapSphereNonAlloc(__instance._fpcModule.Position, 0.8f, DetectedColliders, 16384);
                for (int i = 0; i < num2; i++)
                {
                    if (DetectedColliders[i].TryGetComponent<BreakableWindow>(out var component))
                    {
                        component.Damage(component.health, __instance.ScpRole.DamageHandler, Vector3.zero);
                    }
                }

                ReferenceHub targetHub = ReferenceHubReaderWriter.ReadReferenceHub(reader);
                if (targetHub != null 
                    && targetHub.roleManager.CurrentRole is HumanRole humanRole 
                    && EventManager.ExecuteEvent(ServerEventType.Scp173SnapPlayer, __instance.Owner, targetHub))
                {
                    var bounds = humanRole.FpcModule.Tracer.GenerateBounds(0.4f, ignoreTeleports: true);
                    bounds.Encapsulate(new Bounds(humanRole.FpcModule.Position, Vector3.up * 2.2f));
                    if (bounds.SqrDistance(__instance._fpcModule.Position) > 1.66f)
                        return false;

                    var scp = __instance.Owner.GetSynapsePlayer();
                    var victime = targetHub.GetSynapsePlayer();
                    var damageHandler = __instance.ScpRole.DamageHandler;
                    var damamge = damageHandler.Damage;
                    var ev = new Scp173AttackEvent(scp, victime, damamge, true);

                    _scp.Scp173Attack.RaiseSafely(ev);

                    if (!ev.Allow) return false;

                    damageHandler.Damage = ev.Damage;

                    if (targetHub.playerStats.DealDamage(__instance.ScpRole.DamageHandler) && __instance.ScpRole.SubroutineModule.TryGetSubroutine<Scp173AudioPlayer>(out var subroutine))
                    {
                        Hitmarker.SendHitmarker(__instance.Owner, 1f);
                        subroutine.ServerSendSound(Scp173AudioPlayer.Scp173SoundId.Snap);
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp173AttackTp failed\n" + ex);
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
    public static void OnProcessDamage(Scp939DamageHandler __instance, ReferenceHub ply)
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