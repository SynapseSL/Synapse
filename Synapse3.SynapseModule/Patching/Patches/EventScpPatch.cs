﻿using Achievements;
using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Items.MicroHID;
using MapGeneration;
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
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils.Networking;
using static PlayerRoles.PlayableScps.Scp173.Scp173TeleportAbility;
using static PocketDimensionTeleport;

namespace Synapse3.SynapseModule.Patching.Patches;

[Automatic]
[SynapsePatch("Scp0492Attack", PatchType.ScpEvent)]
public static class Scp0492AttackPatch
{
    private static readonly ScpEvents _scp;
    static Scp0492AttackPatch() => _scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScpAttackAbilityBase<ZombieRole>), nameof(ScpAttackAbilityBase<ZombieRole>.ServerPerformAttack))]
    public static bool OnServerPerformAttack(ZombieAttackAbility __instance)
    {
        try
        {
            var scp = __instance.GetSynapsePlayer();
            var num = Physics.OverlapSphereNonAlloc(__instance.OverlapSphereOrigin, __instance._detectionRadius, ZombieAttackAbility.DetectionsNonAlloc, ZombieAttackAbility.DetectionMask);
            __instance._syncAttack = AttackResult.None;
            
            for (var i = 0; i < num; i++)
            {
                var gameObject = ZombieAttackAbility.DetectionsNonAlloc[i];
                if (!gameObject.TryGetComponent<IDestructible>(out var destructible)
                    || Physics.Linecast(__instance.PlyCam.position, destructible.CenterOfMass, ZombieAttackAbility.BlockerMask)) 
                    continue;
                var hitBox = destructible as HitboxIdentity;
                
                if(hitBox != null && !ZombieAttackAbility.TargettedPlayers.Remove(hitBox.TargetHub)) continue;

                var damage = __instance.DamageAmount;
                if (hitBox != null)
                {
                    var ev = new Scp0492AttackEvent(scp, hitBox.GetSynapsePlayer(), __instance.DamageAmount);
                    _scp.Scp0492Attack.RaiseSafely(ev);
                    if (!ev.Allow) continue;
                    damage = ev.Damage;
                }

                if (!destructible.Damage(damage,
                        new Scp049DamageHandler(scp.Hub, damage, Scp049DamageHandler.AttackType.Scp0492),
                        destructible.CenterOfMass)) continue;
                
                //OnDamage
                __instance.OnDestructibleDamaged(destructible);
                __instance._syncAttack |= AttackResult.AttackedObject;
                if (hitBox == null) continue;
                __instance._syncAttack |= AttackResult.AttackedHuman;
                if (hitBox.TargetHub.playerStats.GetModule<HealthStat>().CurValue <= 0f)
                    __instance._syncAttack |= AttackResult.KilledHuman;
            }
            __instance.ServerSendRpc(toAll: true);
            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error("Sy3 Patch: Scp0492Attack failed\n" + e);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp096Attack", PatchType.ScpEvent)]
public static class Scp096AttackPatch
{
    private static readonly ScpEvents _scp;
    static Scp096AttackPatch() => _scp = Synapse.Get<ScpEvents>();
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp096HitHandler), nameof(Scp096HitHandler.ProcessHits))]
    public static bool ProcessHits(Scp096HitHandler __instance, ref Scp096HitResult __result, int count)
    {
        try
        {
            var scp096HitResult = Scp096HitResult.None;
            var scp = __instance._scpRole._lastOwner.GetSynapsePlayer(); 
            for (var i = 0; i < count; i++)
            {
                var collider = Scp096HitHandler.Hits[i];
                __instance.CheckDoorHit(collider);
                if (!collider.TryGetComponent<IDestructible>(out var destructible))
                    continue;

                var layerMask = (int)Scp096HitHandler.SolidObjectMask & ~(1 << collider.gameObject.layer);
                if (Physics.Linecast(__instance._scpRole.CameraPosition, destructible.CenterOfMass, layerMask) ||
                    !__instance._hitNetIDs.Add(destructible.NetworkId))
                    continue;

                if (destructible is BreakableWindow breakableWindow)
                {
                    if (!__instance.DealDamage(breakableWindow, __instance._windowDamage)) continue;
                    
                    scp096HitResult |= Scp096HitResult.Window;
                    Synapse3Extensions.RaiseEvent(__instance, nameof(Scp096HitHandler.OnWindowHit), breakableWindow);
                    continue;
                }
                
                if (destructible is not HitboxIdentity hitBoxIdentity || !__instance.IsHumanHitbox(hitBoxIdentity))
                        continue;

                var target = hitBoxIdentity.TargetHub?.GetSynapsePlayer();
                var isTarget = __instance._targetCounter.HasTarget(target);
                var damage = isTarget ? __instance._humanTargetDamage : __instance._humanNontargetDamage;

                var ev = new Scp096AttackEvent(scp, target, __instance._damageType, damage);
                _scp.Scp096Attack.RaiseSafely(ev);
                if (!ev.Allow) continue;
                if (!__instance.DealDamage(hitBoxIdentity, ev.Damage)) continue;
                
                scp096HitResult |= Scp096HitResult.Human;
                Synapse3Extensions.RaiseEvent(__instance, nameof(Scp096HitHandler.OnPlayerHit), target.Hub);
                if (!target.Hub.IsAlive())
                {
                    scp096HitResult |= Scp096HitResult.Lethal;
                }
            }

            __instance.HitResult |= scp096HitResult;
            __result = scp096HitResult;
            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error("Sy3 Event: Scp096Attack failed\n" + e);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp049Attack", PatchType.ScpEvent)]
public static class Scp049AttackPatch
{
    private static readonly ScpEvents _scp;
    static Scp049AttackPatch() => _scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp049AttackAbility), nameof(Scp049AttackAbility.ServerProcessCmd))]
    public static bool ServerProcessCmd(Scp049AttackAbility __instance, NetworkReader reader)
    {
        try
        {
            if (!__instance.Cooldown.IsReady || __instance._resurrect.IsInProgress)
            {
                return false;
            }

            __instance._target = reader.ReadReferenceHub();
            if (__instance._target == null || !__instance.IsTargetValid(__instance._target)) return false;
            var effect = __instance._target.playerEffectsController.GetEffect<CardiacArrest>();

            var scp = __instance.Owner.GetSynapsePlayer();
            var victim = __instance._target.GetSynapsePlayer();
            var damage = 0f;
            var enableCardiac = true;
            if (effect.IsEnabled)
            {
                enableCardiac = false;
                damage = -1;
            }
            
            var ev = new Scp049AttackEvent(scp, victim, damage, 1.5f, enableCardiac);
            _scp.Scp049Attack.RaiseSafely(ev);
            if (!ev.Allow) return false;

            if (ev.Cooldown > 0f)
                __instance.Cooldown.Trigger(ev.Cooldown);
            
            if (ev.Damage != 0)
            {
                __instance._target.playerStats.DealDamage(new Scp049DamageHandler(__instance.Owner, ev.Damage,
                    Scp049DamageHandler.AttackType.Instakill));
            }

            if (ev.EnableCardiacEffect && victim.Hub.IsAlive())
            {
                effect.SetAttacker(__instance.Owner);
                effect.Intensity = 1;
                effect.ServerChangeDuration(__instance._statusEffectDuration);
            }

            Synapse3Extensions.RaiseEvent(__instance, nameof(Scp049AttackAbility.OnServerHit), __instance._target);
            __instance.ServerSendRpc(true);
            Hitmarker.SendHitmarker(__instance.Owner, 1f);
            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error("Sy3 Event: Scp049Attack failed\n" + e);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp106Attack", PatchType.ScpEvent)]
public static class Scp106AttackPatch
{
    private static readonly ScpEvents _scp;
    static Scp106AttackPatch() => _scp = Synapse.Get<ScpEvents>();


    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp106Attack), nameof(Scp106Attack.ServerShoot))]
    public static bool OnServerShoot(Scp106Attack __instance)
    {
        try
        {
            Scp106AttackEvent ev;
            var scp = __instance.Owner.GetSynapsePlayer();
            var victim =__instance._targetHub.GetSynapsePlayer();
            
            using (new FpcBacktracker(scp, __instance._targetPosition, 0.35f))
            {
                var vector = __instance._targetPosition - __instance._ownerPosition;
                var sqrMagnitude = vector.sqrMagnitude;
                if (sqrMagnitude > __instance._maxRangeSqr)
                    return false;

                var forward = __instance.OwnerCam.forward;
                forward.y = 0f;
                vector.y = 0f;
                if (Physics.Linecast(__instance._ownerPosition, __instance._targetPosition, MicroHIDItem.WallMask))
                    return false;

                if (__instance._dotOverDistance.Evaluate(sqrMagnitude) > Vector3.Dot(vector.normalized, forward.normalized))
                {
                    ev = new Scp106AttackEvent(scp, null, 0, false, __instance._missCooldown);
                    _scp.Scp106Attack.RaiseSafely(ev);
                    if (!ev.Allow) return false;
                    __instance.SendCooldown(ev.Cooldown);
                    return false;
                }

                ev = new Scp106AttackEvent(scp, victim, __instance._damage, true, __instance._hitCooldown);
                _scp.Scp106Attack.RaiseSafely(ev);

                if (!EventManager.ExecuteEvent(ServerEventType.Scp106TeleportPlayer, __instance.Owner, __instance._targetHub))
                    return false;
                
                if (!ev.Allow) return false;

                var handler = new ScpDamageHandler(__instance.Owner, ev.Damage, DeathTranslations.PocketDecay);
                if (!__instance._targetHub.playerStats.DealDamage(handler))
                {
                    return false;
                }
            }

            __instance.SendCooldown(ev.Cooldown);
            __instance.Vigor.VigorAmount += 0.3f;
            __instance.ReduceSinkholeCooldown();
            Hitmarker.SendHitmarker(scp, 1f);
            if (!ev.TakeToPocket) return false;
            
            Synapse3Extensions.RaiseEvent(typeof(Scp106Attack), nameof(Scp106Attack.OnPlayerTeleported), victim.Hub);
            var playerEffectsController = victim.PlayerEffectsController;
            playerEffectsController.EnableEffect<Traumatized>(180f);
            playerEffectsController.EnableEffect<Corroding>();
            scp.MainScpController.Scp106.PlayersInPocket.Add(ev.Victim);
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Sy3 Event: Scp106Attack failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp173AttackSnap", PatchType.ScpEvent)]
public static class Scp173AttackSnapPatch
{
    private static readonly ScpEvents _scp;
    static Scp173AttackSnapPatch() => _scp = Synapse.Get<ScpEvents>();
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173SnapAbility), nameof(Scp173SnapAbility.ServerProcessCmd))]
    public static bool OnServerProcessCmd(Scp173SnapAbility __instance, NetworkReader reader)
    {
        try
        {
            __instance._targetHub = reader.ReadReferenceHub();
            if (__instance._observersTracker.IsObserved 
                || __instance._targetHub == null 
                || __instance._targetHub.roleManager.CurrentRole is not IFpcRole currentRole 
                || __instance.IsSpeeding)
            {
                return false;
            }
            var scpCameraReference = __instance.Owner.PlayerCameraReference;
            
            var scpFpcModule = __instance.ScpRole.FpcModule;
            var victimFpcModule = currentRole.FpcModule;

            var victimPosition = victimFpcModule.Position;
            var scpPosition = scpFpcModule.Position;
            
            var scpRotation = scpCameraReference.rotation;
            
            victimFpcModule.Position = victimFpcModule.Tracer.GenerateBounds(0.4f, true).ClosestPoint(reader.ReadRelativePosition().Position);
            var bounds = scpFpcModule.Tracer.GenerateBounds(0.1f, true);
            bounds.Encapsulate(scpFpcModule.Position + scpFpcModule.Motor.Velocity * 0.2f);
            scpFpcModule.Position = bounds.ClosestPoint(reader.ReadRelativePosition().Position);
            scpCameraReference.rotation = reader.ReadLowPrecisionQuaternion().Value;

            if (Scp173SnapAbility.TryHitTarget(scpCameraReference, out var target))
            {
                var scp = __instance.Owner.GetSynapsePlayer();
                var ev = new Scp173AttackEvent(scp, target.GetSynapsePlayer(), -1, false);
                _scp.Scp173Attack.RaiseSafely(ev);
                var damageHandler = new ScpDamageHandler(scp, ev.Damage, DeathTranslations.Scp173);
            
                if (!EventManager.ExecuteEvent(ServerEventType.Scp173SnapPlayer, __instance.Owner, __instance._targetHub))
                    return false;

                if (ev.Allow)
                {
                    if (target.playerStats.DealDamage(damageHandler))
                    {
                        Hitmarker.SendHitmarker(__instance.Owner, 1f);
                        if (__instance.ScpRole.SubroutineModule.TryGetSubroutine<Scp173AudioPlayer>(out var subroutine))
                        {
                            subroutine.ServerSendSound(Scp173AudioPlayer.Scp173SoundId.Snap);
                        }
                    }   
                }   
            }

            victimFpcModule.Position = victimPosition;
            scpFpcModule.Position = scpPosition;
            scpCameraReference.rotation = scpRotation;
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Sy3 Event: Scp173AttackSnap failed\n" + ex);
            //Depending on where the error was thrown is the Network reader already partially read so that the base game would throw an error and kick SCP-173
            return false;
        }
    }
}


[Automatic]
[SynapsePatch("Scp173AttackTp", PatchType.ScpEvent)]
public static class Scp173AttackTpPatch
{
    private static readonly ScpEvents _scp;
    static Scp173AttackTpPatch() => _scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173TeleportAbility), nameof(Scp173TeleportAbility.ServerProcessCmd))]
    public static bool OnServerProcessCmd(Scp173TeleportAbility __instance, NetworkReader reader)
    {
        try
        {
            __instance._cmdData = (CmdTeleportData)reader.ReadByte();
            if (!__instance.HasDataFlag(CmdTeleportData.WantsToTeleport))
            {
                __instance.ServerSendRpc(true);
                return false;
            }

            if (!__instance._blinkTimer.AbilityReady)
                return false;

            var playerCameraReference = __instance.Owner.PlayerCameraReference;
            var prevObservers = new HashSet<ReferenceHub>(__instance._observersTracker.Observers);
            var cmdData = __instance._cmdData;
            __instance._cmdData = 0;
            __instance.ServerSendRpc(true);
            __instance._cmdData = cmdData;
            var rotation = playerCameraReference.rotation;
            playerCameraReference.rotation = reader.ReadQuaternion();
            var canBlink = __instance.TryBlink(reader.ReadSingle());
            playerCameraReference.rotation = rotation;
            if (!canBlink)
                return false;

            prevObservers.UnionWith(__instance._observersTracker.Observers);
            __instance.ServerSendRpc(x => prevObservers.Contains(x));
            __instance._audioSubroutine.ServerSendSound(Scp173AudioPlayer.Scp173SoundId.Teleport);
            if (__instance._breakneckSpeedsAbility.IsActive)
                return false;

            var objectCount =
                Physics.OverlapSphereNonAlloc(__instance._fpcModule.Position, 0.8f, DetectedColliders, 16384);
            
            for (var i = 0; i < objectCount; i++)
            {
                if (DetectedColliders[i].TryGetComponent<BreakableWindow>(out var component))
                {
                    component.Damage(component.health, __instance.ScpRole.DamageHandler, Vector3.zero);
                }
            }

            var targetHub = reader.ReadReferenceHub();

            if (targetHub == null || targetHub.roleManager.CurrentRole is not FpcStandardRoleBase fpcRole) return false;


            var bounds = fpcRole.FpcModule.Tracer.GenerateBounds(0.4f, true);
            bounds.Encapsulate(new Bounds(fpcRole.FpcModule.Position, Vector3.up * 2.2f));
            if (bounds.SqrDistance(__instance._fpcModule.Position) > 1.66f)
                return false;

            var scp = __instance.Owner.GetSynapsePlayer();
            var ev = new Scp173AttackEvent(scp, targetHub.GetSynapsePlayer(), -1, true);
            _scp.Scp173Attack.RaiseSafely(ev);
            if (EventManager.ExecuteEvent(ServerEventType.Scp173SnapPlayer, __instance.Owner, targetHub))
                return false;
            if (!ev.Allow) return false;

            if (!targetHub.playerStats.DealDamage(new ScpDamageHandler(scp, ev.Damage, DeathTranslations.Scp173)))
                return false;

            if (!__instance.ScpRole.SubroutineModule.TryGetSubroutine<Scp173AudioPlayer>(out var audioPlayer))
                return false;
            
            Hitmarker.SendHitmarker(__instance.Owner, 1f);
            audioPlayer.ServerSendSound(Scp173AudioPlayer.Scp173SoundId.Snap);

            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Sy3 Event: Scp173AttackTp failed\n" + ex);
            //Depending on where the error was thrown is the Network reader already partially read so that the base game would throw an error and kick SCP-173
            return false;
        }
    }
}

//TODO: Check Events
[Automatic]
[SynapsePatch("Scp939Lunge", PatchType.ScpEvent)]
public static class Scp939LunchPatch
{
    private static readonly ScpEvents _scp;
    static Scp939LunchPatch() => _scp = Synapse.Get<ScpEvents>();
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp939DamageHandler), nameof(Scp939DamageHandler.ProcessDamage))]
    public static bool OnProcessDamage(Scp939DamageHandler __instance, ReferenceHub ply)
    {
        try
        {
            var scp = __instance.Attacker.GetSynapsePlayer();
            var victim = ply.GetSynapsePlayer();
            var ev =  new Scp939AttackEvent(scp, victim, __instance.Damage, __instance._damageType);
            _scp.Scp939Attack.RaiseSafely(ev);
            __instance.Damage = ev.Damage;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp939Lunge failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerEscapePocketDimension", PatchType.ScpEvent)]
public static class PlayerEscapePocketDimensionPatch
{
    private static readonly ScpEvents _scp;
    static PlayerEscapePocketDimensionPatch() => _scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PocketDimensionTeleport), nameof(PocketDimensionTeleport.OnTriggerEnter))]
    public static bool OnTriggerEnter(PocketDimensionTeleport __instance, Collider other)
    {
        try
        {
            var component = other.GetComponent<NetworkIdentity>();
            if (component == null 
                || !ReferenceHub.TryGetHubNetID(component.netId, out var hub) 
                || hub.roleManager.CurrentRole.ActiveTime < 1f)
                return false;

            var player = hub.GetSynapsePlayer();
            var fpcRole = player.CurrentRole as IFpcRole;
            if (fpcRole == null) return false;
            var escapePosition = Scp106PocketExitFinder.GetBestExitPosition(fpcRole);
            var enteredPosition = __instance.transform.position;

            var escape = __instance._type == PDTeleportType.Exit || AlphaWarheadController.Detonated;
            var ev = new Scp106LeavePocketEvent(player, escape, enteredPosition, escapePosition);

            _scp.Scp106LeavePocket.RaiseSafely(ev);

            if (!ev.Allow) return false;

            if (!ev.EscapePocket)
            {
                if (!EventManager.ExecuteEvent(ServerEventType.PlayerExitPocketDimension, hub, false))
                    return false;
                hub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.PocketDecay));
            }
            else
            {
                if (!EventManager.ExecuteEvent(ServerEventType.PlayerExitPocketDimension, hub, true))
                    return false;
                fpcRole.FpcModule.ServerOverridePosition(ev.EscapePosition, Vector3.zero);
                hub.playerEffectsController.EnableEffect<Disabled>(10f, addDuration: true);
                hub.playerEffectsController.DisableEffect<Corroding>();
                AchievementHandlerBase.ServerAchieve(component.connectionToClient, AchievementName.LarryFriend);
                ImageGenerator.pocketDimensionGenerator.GenerateRandom();
            }

            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"Sy3 Event: PlayerEscapePocketDimension failed!!\n{e}");
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp173Tantrum", PatchType.ScpEvent)]
public static class Scp173TantrumPatch
{
    private static readonly ScpEvents _scp;
    static Scp173TantrumPatch() => _scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173TantrumAbility), nameof(Scp173TantrumAbility.ServerProcessCmd))]
    public static bool OnServerProcessCmd(Scp173TantrumAbility __instance, NetworkReader reader)
    {
        try
        {
            if (!__instance.Cooldown.IsReady
                || __instance._observersTracker.IsObserved
                || !Physics.Raycast(__instance.ScpRole.FpcModule.Position, Vector3.down, out var hitInfo, 3f, __instance._tantrumMask)
                || !EventManager.ExecuteEvent(ServerEventType.Scp173CreateTantrum, __instance.Owner))
            {
                return false;
            }

            var player = __instance.ScpRole._owner.GetSynapsePlayer();

            var ev = new Scp173PlaceTantrumEvent(player, player.MainScpController.Scp173.TantrumCoolDown);

            _scp.Scp173PlaceTantrum.RaiseSafely(ev);

            if (!ev.Allow) return false;
            
            __instance.Cooldown.Trigger(ev.CoolDown);
            __instance.ServerSendRpc(toAll: true);
            var tantrumEnvironmentalHazard = UnityEngine.Object.Instantiate(__instance._tantrumPrefab);
            var targetPos = hitInfo.point + Vector3.up * 1.25f;
            tantrumEnvironmentalHazard.SynchronizedPosition = new RelativePosition(targetPos);
            NetworkServer.Spawn(tantrumEnvironmentalHazard.gameObject);
            foreach (TeslaGate teslaGate in TeslaGateController.Singleton.TeslaGates)
            {
                if (teslaGate.PlayerInIdleRange(__instance.Owner))
                {
                    teslaGate.TantrumsToBeDestroyed.Add(tantrumEnvironmentalHazard);
                }
            }
            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"Sy3 Event: Scp173Tantrum failed!!\n{e}");
            throw;
        }
    }
}


[Automatic]
[SynapsePatch("Scp173Observe", PatchType.ScpEvent)]
public static class Scp173ObservePatch
{
    private static readonly ScpEvents _scp;
    static Scp173ObservePatch() => _scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173TantrumAbility), nameof(Scp173TantrumAbility.ServerProcessCmd))]
    public static bool OnServerProcessCmd(Scp173TantrumAbility __instance, NetworkReader reader)
    {
        try
        {
            if (!__instance.Cooldown.IsReady
                || __instance._observersTracker.IsObserved
                || !Physics.Raycast(__instance.ScpRole.FpcModule.Position, Vector3.down, out var hitInfo, 3f, __instance._tantrumMask)
                || !EventManager.ExecuteEvent(ServerEventType.Scp173CreateTantrum, __instance.Owner))
            {
                return false;
            }

            var player = __instance.ScpRole._owner.GetSynapsePlayer();

            var ev = new Scp173PlaceTantrumEvent(player, player.MainScpController.Scp173.TantrumCoolDown);

            _scp.Scp173PlaceTantrum.RaiseSafely(ev);

            if (!ev.Allow) return false;

            __instance.Cooldown.Trigger(ev.CoolDown);
            __instance.ServerSendRpc(toAll: true);
            var tantrumEnvironmentalHazard = UnityEngine.Object.Instantiate(__instance._tantrumPrefab);
            var targetPos = hitInfo.point + Vector3.up * 1.25f;
            tantrumEnvironmentalHazard.SynchronizedPosition = new RelativePosition(targetPos);
            NetworkServer.Spawn(tantrumEnvironmentalHazard.gameObject);
            foreach (TeslaGate teslaGate in TeslaGateController.Singleton.TeslaGates)
            {
                if (teslaGate.PlayerInIdleRange(__instance.Owner))
                {
                    teslaGate.TantrumsToBeDestroyed.Add(tantrumEnvironmentalHazard);
                }
            }
            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"Sy3 Event: Scp173Observe failed!!\n{e}");
            throw;
        }
    }
}

[Automatic]
[SynapsePatch("Scp173ObserversTracker", PatchType.Wrapper)]
public static class Scp173ObserversTrackerPatch
{
    private static readonly SynapseConfigService _config;
    private static readonly ScpEvents _scp;
    static Scp173ObserversTrackerPatch()
    {
        _config = Synapse.Get<SynapseConfigService>();
        _scp = Synapse.Get<ScpEvents>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.IsObservedBy))]
    public static bool IsObservedBy(Scp173ObserversTracker __instance, ref bool __result, ReferenceHub target)
    {
        var player = target.GetSynapsePlayer();
        var scp = __instance.Owner.GetSynapsePlayer();
        var ev = new Scp173ObserveEvent(player, true, scp);

        _scp.Scp173Observe.RaiseSafely(ev);

        if (!ev.Allow || _config.GamePlayConfiguration.CantObserve173.Contains(player.RoleID))
        {
            __result = false;
            return false;
        }
        return true;
    }
}