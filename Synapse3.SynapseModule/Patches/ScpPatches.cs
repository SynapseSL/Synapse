using System;
using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using GameCore;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.MicroHID;
using MapGeneration;
using Neuron.Core.Logging;
using PlayableScps;
using PlayableScps.Messages;
using PlayerStatsSystem;
using Subtitles;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using Utils.Networking;
using Console = GameCore.Console;
using Object = UnityEngine.Object;

namespace Synapse3.SynapseModule.Patches;

[Patches]
[HarmonyPatch]
internal static class ScpPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp049), nameof(Scp049.BodyCmd_ByteAndGameObject))]
    public static bool Scp049Attack(Scp049 __instance, byte num, GameObject go)
    {
        try
        {
            DecoratedScpPatches.Scp049AttackAndRevive(__instance, num, go);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp049Attack/Revive Event Failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp049_2PlayerScript), nameof(Scp049_2PlayerScript.UserCode_CmdHurtPlayer))]
    public static bool Scp0492Attack(Scp049_2PlayerScript __instance, GameObject plyObj)
    {
        try
        {
            DecoratedScpPatches.Scp0492Attack(__instance, plyObj);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp0492 Attack Event Failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173), nameof(Scp173.UpdateObservers))]
    public static bool ObserveScp173(Scp173 __instance)
    {
        try
        {
            DecoratedScpPatches.ObserveScp173(__instance);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp173 Observe Event Failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173), nameof(Scp173.ServerKillPlayer))]
    public static bool Scp173Attack(Scp173 __instance, ReferenceHub target)
    {
        try
        {
            DecoratedScpPatches.Scp173Attack(__instance, target);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp173 Attack Event Failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp939), nameof(Scp939.ServerAttack))]
    public static bool Scp939Attack(Scp939 __instance, GameObject target, out bool __result)
    {
        try
        {
            __result = DecoratedScpPatches.Scp939Attack(__instance, target);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp939 Attack Event Failed\n" + ex);
            __result = false;
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173),nameof(Scp173.ServerDoBreakneckSpeeds))]
    public static bool ActivateBreakneckSpeed(Scp173 __instance)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (player == null || __instance._breakneckSpeedsCooldownRemaining > 0f) return false;
            var ev = new Scp173ActivateBreakneckSpeedEvent(player);
            Synapse.Get<ScpEvents>().Scp173ActivateBreakneckSpeed.Raise(ev);
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Activate Breakneck Speed Event Failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173),nameof(Scp173.ServerDoTantrum))]
    public static bool PlaceTantrum(Scp173 __instance)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (player == null || __instance._tantrumCooldownRemaining > 0f || __instance._isObserved) return false;
            var ev = new Scp173PlaceTantrumEvent(player);
            Synapse.Get<ScpEvents>().Scp173PlaceTantrum.Raise(ev);
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Place Tantrum Event Failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.UserCode_CmdContain106))]
    public static bool Contain106(PlayerInteract __instance)
    {
        try
        {
            return DecoratedScpPatches.Contain106(__instance);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Contain Scp 106 Event Failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.UserCode_CmdMakePortal))]
    public static bool CreatePortal(Scp106PlayerScript __instance)
    {
        try
        {
            DecoratedScpPatches.CreatePortal(__instance);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Create Portal Event Failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.UserCode_CmdMovePlayer))]
    public static bool Scp106Attack(Scp106PlayerScript __instance, GameObject ply, int t)
    {
        try
        {
            DecoratedScpPatches.Scp106Attack(__instance, ply, t);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp 106 Attack Event Failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PocketDimensionTeleport), nameof(PocketDimensionTeleport.OnTriggerEnter))]
    public static bool LeavePocket(PocketDimensionTeleport __instance, Collider other)
    {
        try
        {
            DecoratedScpPatches.LeavePocket(__instance, other);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp 106 Attack Event Failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.UpdateVision))]
    public static bool Vision096(Scp096 __instance)
    {
        try
        {
            DecoratedScpPatches.Vision096(__instance);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Observe Scp 096 Event Failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.OnDamage))]
    public static bool Damage096(Scp096 __instance, DamageHandlerBase handler)
    {
        try
        {
            if (handler is AttackerDamageHandler attackerDamageHandler && attackerDamageHandler.Attacker.Hub != null &&
                __instance.CanEnrage)
            {
                var scp = __instance.GetSynapsePlayer();
                var player = attackerDamageHandler.Attacker.GetSynapsePlayer();
                if (scp == null || player == null || player == scp) return false;
                
                var ev = new Scp096ObserveEvent(player,
                    player.Invisible < InvisibleMode.Ghost && !Synapse.Get<SynapseConfigService>().GamePlayConfiguration.CantObserve096
                        .Contains(player.RoleID), scp);
                Synapse.Get<ScpEvents>().Scp096Observed.Raise(ev);
                if (!ev.Allow) return false;
                __instance.AddTarget(player.gameObject);
                __instance.Windup();
            }

            __instance.Shield.SustainTime = 25f;
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Observe Scp 096 (Damage) Event Failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.ServerHitObject))]
    public static bool Scp096HitObject(Scp096 __instance, GameObject target,out bool __result)
    {
        try
        {
            __result = DecoratedScpPatches.Hit096(__instance, target);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp096 Attack (HitObject) Event Failed\n" + ex);
            __result = false;
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.ChargePlayer))]
    public static bool Scp096ChargePlayer(Scp096 __instance, ReferenceHub player)
    {
        try
        {
            DecoratedScpPatches.ChargePlayer(__instance, player);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp096 Attack (Charge) Event Failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.UpdatePry))]
    public static bool Scp096Pry(Scp096 __instance)
    {
        try
        {
            DecoratedScpPatches.Pry(__instance);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp096 Attack (Pry) Event Failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Recontainer079), nameof(Recontainer079.UpdateStatus))]
    public static bool UpdateContain079Status(Recontainer079 __instance, int engagedGenerators)
    {
        try
        {
            DecoratedScpPatches.UpdateStatus(__instance, engagedGenerators);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp079 Contain (OverrideDoors) Event Failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Recontainer079), nameof(Recontainer079.EndOvercharge))]
    public static void End079Contain()
    {
        try
        {
            var ev = new Scp079ContainEvent(Scp079ContainmentStatus.Finished);
            Synapse.Get<ScpEvents>().Scp079Contain.Raise(ev);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp079 Contain (End) Event Failed\n" + ex);
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Recontainer079), nameof(Recontainer079.Recontain))]
    public static bool Start079Contain()
    {
        try
        {
            var ev = new Scp079ContainEvent(Scp079ContainmentStatus.AnnounceOvercharge);
            Synapse.Get<ScpEvents>().Scp079Contain.Raise(ev);
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp079 Contain (Start) Event Failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Recontainer079), nameof(Recontainer079.RefreshActivator))]
    public static bool Refresh079ContainActivator(Recontainer079 __instance)
    {
        try
        {
            if (!__instance._delayStopwatch.IsRunning ||
                __instance._delayStopwatch.Elapsed.TotalSeconds <= __instance._activationDelay) return true;
            
            var ev = new Scp079ContainEvent(Scp079ContainmentStatus.Overcharge);
            Synapse.Get<ScpEvents>().Scp079Contain.Raise(ev);
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp079 Contain (Refresh) Event Failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.RpcSwitchCamera))]
    public static bool SwitchCamera(Scp079PlayerScript __instance, ref ushort camId)
    {
        try
        {
            var id = camId;
            var player = __instance.GetSynapsePlayer();
            var camera = Synapse.Get<MapService>()._synapseCameras.FirstOrDefault(x => x.CameraID == id);
            if (player == null || camera == null) return false;

            var ev = new Scp079SwitchCameraEvent(!player.ScpController.Scp079.Spawned, player, camera);
            Synapse.Get<ScpEvents>().Scp079SwitchCamera.Raise(ev);
            
            if (ev.Spawning)
                player.ScpController.Scp079.Spawned = true;

            camId = ev.Camera.CameraID;

            return ev.Allow || ev.Spawning;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Switch Camera Event Failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.UserCode_CmdInteract))]
    public static bool Scp079BulkPatch(Scp079PlayerScript __instance, Command079 command, string args, GameObject target)
    {
        try
        {
            return DecoratedScpPatches.Scp079BulkPatch(__instance, command, args, target);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp079 Bulk Patch Failed\n" + ex);
            return true;
        }
    }
}

internal static class DecoratedScpPatches
{
    public static bool Scp079BulkPatch(Scp079PlayerScript script, Command079 command, string args,
        GameObject gameObject)
    {
        if (!script._interactRateLimit.CanExecute() || !script.iAm079) return false;
        Console.AddDebugLog("SCP079", "Command received from a client: " + command, MessageImportance.LessImportant);
        script.RefreshCurrentRoom();
        if (!script.CheckInteractableLegitness(script.CurrentRoom, gameObject, true)) return false;
        var player = script.GetSynapsePlayer();
        if (player == null) return false;
        
        switch (command)
        {
            case Command079.Door:
                if(AlphaWarheadController.Host.inProgress) return false;
                if(gameObject == null || !gameObject.TryGetComponent<DoorVariant>(out var doorVariant)) return false;

                var door = doorVariant.GetSynapseDoor();
                if (door == null) return false;
                
                var blacklistedDoors = ConfigFile.ServerConfig.GetStringList("scp079_door_blacklist") ?? new List<string>();
                if (blacklistedDoors.Contains(door.Name)) return false;
                var permission = doorVariant.RequiredPermissions.RequiredPermissions.ToString();
                var mana = script.GetManaFromLabel("Door Interaction " + permission.Split(',')[0], script.abilities);

                var ev = new Scp079DoorInteractEvent(player, door, mana);
                Synapse.Get<ScpEvents>().Scp079DoorInteract.Raise(ev);
                
                if (ev.Energy > script._curMana)
                {
                    script.RpcNotEnoughMana(mana, script._curMana);
                    return false;
                }

                if (!ev.Allow) return false;

                var state = doorVariant.TargetState;
                doorVariant.ServerInteract(player, 0);
                if (state == doorVariant.TargetState) return false;
                
                script.Mana -= ev.Energy;
                script.AddInteractionToHistory(gameObject, true);
                return false;
            
            case Command079.Doorlock:
                if(AlphaWarheadController.Host.inProgress) return false;
                if(gameObject == null || !gameObject.TryGetComponent(out doorVariant)) return false;

                door = doorVariant.GetSynapseDoor();
                if (door == null) return false;
                
                blacklistedDoors = ConfigFile.ServerConfig.GetStringList("scp079_door_blacklist") ?? new List<string>();
                if (blacklistedDoors.Contains(door.Name)) return false;

                var mode = DoorLockUtils.GetMode((DoorLockReason)doorVariant.ActiveLocks);
                if (!mode.HasFlagFast(DoorLockMode.CanOpen) && !mode.HasFlagFast(DoorLockMode.CanClose) &&
                    !mode.HasFlagFast(DoorLockMode.ScpOverride)) return false;

                var ev2 = new Scp079LockDoorEvent(player, door,
                    ((DoorLockReason)doorVariant.ActiveLocks).HasFlag(DoorLockReason.Regular079),
                    script.GetManaFromLabel("Door Lock Minimum", script.abilities));
                Synapse.Get<ScpEvents>().Scp079LockDoor.Raise(ev2);
                
                if (!ev2.Allow) return false;
                
                if (ev2.Unlock)
                {
                    if (!script.lockedDoors.Contains(doorVariant.netId)) return false;
                    script.lockedDoors.Remove(doorVariant.netId);
                    doorVariant.ServerChangeLock(DoorLockReason.Regular079, false);
                }
                else
                {
                    if (ev2.Energy > script.Mana)
                    {
                        script.RpcNotEnoughMana(ev2.Energy, script._curMana);
                        return false;
                    }

                    if (!script.lockedDoors.Contains(doorVariant.netId))
                        script.lockedDoors.Add(doorVariant.netId);

                    doorVariant.ServerChangeLock(DoorLockReason.Regular079, true);
                    script.AddInteractionToHistory(doorVariant.gameObject, true);
                    script.Mana -= ev2.Energy;
                }
                return false;
            
            case Command079.Speaker:
                var speakerName = script.CurrentRoom.transform.parent.name + "/" + script.CurrentRoom.name +
                              "/Scp079Speaker";
                mana = script.GetManaFromLabel("Speaker Start", script.abilities);
                var ev3 = new Scp079StartSpeakerEvent(player, mana, speakerName);
                Synapse.Get<ScpEvents>().Scp079StartSpeaker.Raise(ev3);
                var speaker = GameObject.Find(speakerName);
                if(speaker == null) return false;
                
                if (!ev3.Allow) return false;

                if (ev3.Energy > script._curMana)
                {
                    script.RpcNotEnoughMana(ev3.Energy, script._curMana);
                    return false;
                }

                script.Mana -= ev3.Energy;
                script.Speaker = speakerName;
                script.AddInteractionToHistory(speaker, true);
                return false;
            
            case Command079.StopSpeaker:
                var ev4 = new Scp079StopSpeakerEvent(player, script.Speaker);
                Synapse.Get<ScpEvents>().Scp079StopSpeaker.Raise(ev4);
                if(!ev4.Allow) return false;
                script.Speaker = string.Empty;
                return false;

            //TODO: Add the other 079 Events
            
            default:
                NeuronLogger.For<Synapse>().Debug(command);
                script._interactRateLimit._usages--;
                return true;
        }
    }
    
    public static void UpdateStatus(Recontainer079 container079, int generators)
    {
        var count = Recontainer079.AllGenerators.Count;
        var announcement = string.Format(container079._announcementProgress, generators, count);
        var parts = new List<SubtitlePart>
        {
            new(SubtitleType.GeneratorsActivated, new[] { generators.ToString(), count.ToString() })
        };
        
        if (generators >= count)
        {
            var ev = new Scp079ContainEvent(Scp079ContainmentStatus.OverrideDoors);
            Synapse.Get<ScpEvents>().Scp079Contain.Raise(ev);
            if(!ev.Allow) return;

            announcement += container079._announcementAllActivated;
            container079.SetContainmentDoors(true, Scp079PlayerScript.instances.Count > 0);
            parts.Add(new SubtitlePart(SubtitleType.AllGeneratorsEngaged, null));
        }

        new SubtitleMessage(parts.ToArray()).SendToAuthenticated();
        container079.PlayAnnouncement(announcement, 1f);
    }
    
    public static void Pry(Scp096 scp096)
    {
        if(!scp096.PryingGate) return;
        var scp = scp096.GetSynapsePlayer();
        if (scp == null) return;

        var amountOfPlayers = Physics.OverlapSphereNonAlloc(scp.Position, 0.5f, Scp096._sphereHits, LayerMask.GetMask("Hitbox"));
        if(amountOfPlayers <= 0) return;

        for (int i = 0; i < amountOfPlayers; i++)
        {
            var gameObject = Scp096._sphereHits[i].gameObject;
            var victim = gameObject.GetSynapsePlayer();
            if(victim == null || victim == scp ||victim.GodMode) continue;
            if (!Synapse3Extensions.GetHarmPermission(scp, victim)) continue;

            var damage = new Scp096DamageHandler(scp096, 9696f, Scp096DamageHandler.AttackType.GateKill);
            var ev = new Scp096AttackEvent(scp, victim, damage.Damage);
            Synapse.Get<ScpEvents>().Scp096Attack.Raise(ev);
            if(!ev.Allow) continue;
            damage.Damage = ev.Damage;
            
            if(!victim.PlayerStats.DealDamage(damage)) continue;

            if (ev.RemoveTarget)
                scp096._targets.Remove(victim);

            new Scp096OnKillMessage(scp).SendToAuthenticated();
        }
    }
    
    public static void ChargePlayer(Scp096 scp096, ReferenceHub hub)
    {
        var scp = scp096.GetSynapsePlayer();
        var victim = hub.GetSynapsePlayer();
        if (victim == null || scp == null || victim.GodMode || victim == scp) return;
        if (!Synapse3Extensions.GetHarmPermission(scp, victim)) return;
        if (Physics.Linecast(scp.transform.position, victim.transform.position, Scp096._solidObjectMask)) return;
        if (scp096._chargeHitTargets.Contains(hub)) return;

        var isTarget = scp096._targets.Contains(hub);
        var damage = new Scp096DamageHandler(scp096, isTarget ? 9696f : 40f, Scp096DamageHandler.AttackType.Charge);
        var ev = new Scp096AttackEvent(scp, victim, damage.Damage);
        Synapse.Get<ScpEvents>().Scp096Attack.Raise(ev);
        if(!ev.Allow) return;
        damage.Damage = ev.Damage;

        var didDamage = victim.PlayerStats.DealDamage(damage);
        scp096._chargeHitTargets.Add(hub);

        if (didDamage)
        {
            if (ev.RemoveTarget)
                scp096._targets.Remove(hub);
            Hitmarker.SendHitmarker(scp, 1.35f);
            new Scp096OnKillMessage(scp).SendToAuthenticated();
            if (!scp096._chargeKilled)
            {
                scp096._chargeCooldownPenaltyAmount++;
                scp096._chargeKilled = true;
            }
        }

        if (isTarget)
        {
            scp096.EndChargeNextFrame();
        }
    }
    
    public static bool Hit096(Scp096 scp096, GameObject gameObject)
    {
        if (gameObject.TryGetComponent<BreakableWindow>(out var window))
            return window.Damage(500f, new Scp096DamageHandler(scp096, 500f, Scp096DamageHandler.AttackType.Slap),
                gameObject.transform.position);

        if (gameObject.TryGetComponent<DoorVariant>(out var variant) && variant is IDamageableDoor damageableDoor &&
            !variant.IsConsideredOpen())
            return damageableDoor.ServerDamage(250f, DoorDamageType.Scp096);

        var victim = gameObject.GetSynapsePlayer();
        var scp = scp096.GetSynapsePlayer();

        if (victim == null || scp == null || victim.GodMode || victim == scp ||
            victim.RoleType == RoleType.Spectator) return false;
        if (!Synapse3Extensions.GetHarmPermission(scp, victim)) return false;
        if (Physics.Linecast(scp.Position, victim.Position, Scp096._solidObjectMask)) return false;
        if (Vector3.Distance(victim.Position, scp.Position) > 5f) return false;

        var damage = new Scp096DamageHandler(scp096, 9696f, Scp096DamageHandler.AttackType.Slap);
        var ev = new Scp096AttackEvent(scp, victim, damage.Damage);
        Synapse.Get<ScpEvents>().Scp096Attack.Raise(ev);
        if (!ev.Allow) return false;
        damage.Damage = ev.Damage;
        if (!victim.PlayerStats.DealDamage(damage)) return false;

        if (ev.RemoveTarget)
            scp096._targets.Remove(victim);
        
        new Scp096OnKillMessage(scp).SendToAuthenticated();
        return true;
    }
    
    public static void Vision096(Scp096 scp096)
    {
        var headPos = scp096.Hub.transform.TransformPoint(Scp096._headOffset);
        var scp = scp096.GetSynapsePlayer();
        if(scp == null) return;
        foreach (var player in Synapse.Get<PlayerService>().Players)
        {
            if (player.RoleType == RoleType.Spectator || player == scp096.Hub) continue;
            if (Vector3.Dot((player.CameraReference.position - headPos).normalized,
                    scp096.Hub.PlayerCameraReference.forward) < 0.1f) continue;
            
            var visionInfo = VisionInformation.GetVisionInformation(player, headPos, -0.1f,
                60f, true, true, scp096.Hub.localCurrentRoomEffects);

            if (!visionInfo.IsLooking) continue;
            var ev = new Scp096ObserveEvent(player,
                player.Invisible < InvisibleMode.Ghost && !Synapse.Get<SynapseConfigService>().GamePlayConfiguration.CantObserve096
                    .Contains(player.RoleID), scp);
            Synapse.Get<ScpEvents>().Scp096Observed.Raise(ev);
            if(!ev.Allow) continue;
            
            var delay = visionInfo.LookingAmount / 0.25f * (visionInfo.Distance * 0.1f);
            if (!scp096.Calming)
                scp096.AddTarget(player.gameObject);
            if (scp096.CanEnrage && player.gameObject != null)
                scp096.PreWindup(delay);
        }
    }
    
    public static void LeavePocket(PocketDimensionTeleport exit, Collider other)
    {
        var player = other.gameObject.GetSynapsePlayer();
        if (player == null) return;
        var escape = player.GodMode || exit._type == PocketDimensionTeleport.PDTeleportType.Exit ||
                     !Synapse3Extensions.CanHarmScp(player, false);
        var ev = new Scp106LeavePocketEvent(player, escape, player.ClassManager.Scp106.GrabbedPosition);
        Synapse.Get<ScpEvents>().Scp106LeavePocket.Raise(ev);
        player.ClassManager.Scp106.GrabbedPosition = ev.EnteredPosition;

        foreach (var scp in Synapse.Get<PlayerService>().Players)
        {
            scp.ScpController.Scp106.PlayersInPocket.Remove(player);
        }

        if (ev.EscapePocket)
        {
            exit.SuccessEscape(player);
        }
        else
        {
            player.PlayerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.PocketDecay));
        }
        
        ImageGenerator.pocketDimensionGenerator.GenerateRandom();
    }
    
    public static void Scp106Attack(Scp106PlayerScript script, GameObject ply, int time)
    {
        if(!script._iawRateLimit.CanExecute() || !script.iAm106 || !ServerTime.CheckSynchronization(time))
            return;
        var scp = script.GetSynapsePlayer();
        var victim = ply.GetSynapsePlayer();
        if (scp == null || victim == null || victim.GodMode) return;
        if (!Synapse3Extensions.GetHarmPermission(scp, victim)) return;
        var position = victim.transform.position;
        var distance = Vector3.Distance(scp.Position, position);
        var yDistance = Math.Abs(scp.Position.y - position.y);
        if (distance >= 4.2f || (distance >= 3.1f && yDistance < 1.02f) || (distance >= 3.4f && yDistance < 1.95f) ||
            (distance > 3.7f && yDistance < 2.2) || (distance >= 3.9f && yDistance < 3f))
            return;
        if (Physics.Linecast(scp.Position, victim.transform.position, MicroHIDItem.WallMask))
            return;

        var sendToPocket = !Scp106PlayerScript._blastDoor.isClosed;
        var damage = new ScpDamageHandler(script._hub, sendToPocket ? 40f : -1f, DeathTranslations.PocketDecay);
        var ev = new Scp106AttackEvent(scp, victim, damage.Damage, true, sendToPocket);
        Synapse.Get<ScpEvents>().Scp106Attack.Raise(ev);
        if(!ev.Allow) return;
        damage.Damage = ev.Damage;
        
        scp.ClassManager.RpcPlaceBlood(victim.transform.position, 1, 2f);
        script.TargetHitMarker(script.connectionToClient, script.captureCooldown);
        script._currentServerCooldown = script.captureCooldown;

        foreach (var scp079PlayerScript in Scp079PlayerScript.instances)
        {
            scp079PlayerScript.ServerProcessKillAssist(victim, ExpGainType.PocketAssist);
        }

        victim.PlayerStats.DealDamage(damage);

        if (!ev.TakeToPocket) return;
        victim.ClassManager.Scp106.GrabbedPosition = victim.Position;
        victim.PlayerEffectsController.EnableEffect<Corroding>();
        scp.ScpController.Scp106.PlayersInPocket.Add(victim);
    }
    
    public static void CreatePortal(Scp106PlayerScript script)
    {
        if (!script._interactRateLimit.CanExecute() || !script._hub.playerMovementSync.Grounded) return;
        var scp = script.GetSynapsePlayer();
        if (scp == null || !script.iAm106 || script.goingViaThePortal) return;

        if (!Physics.Raycast(new Ray(script.transform.position, -script.transform.up), out var raycast, 10f,
                script.teleportPlacementMask)) return;

        var ev = new Scp106CreatePortalEvent(scp, raycast.point - Vector3.up);
        Synapse.Get<ScpEvents>().Scp106CreatePortal.Raise(ev);
        if (ev.Allow)
        {
            script.SetPortalPosition(Vector3.zero, ev.Position);
        }
    }
    
    public static bool Contain106(PlayerInteract interact)
    {
        var player = interact.GetSynapsePlayer();
        if (player == null) return false;
        if (!interact.CanInteract ||
            !interact.ChckDis(GameObject.FindGameObjectWithTag("FemurBreaker").transform.position) ||
            !Synapse3Extensions.CanHarmScp(player, true)) return false;

        var ev = new Scp106ContainEvent(player);
        var container = Object.FindObjectOfType<LureSubjectContainer>();
        ev.Allow = container.allowContain;
        Synapse.Get<ScpEvents>().Scp106Contain.Raise(ev);

        if (!ev.Allow) return false;
        container.allowContain = true;
        return true;
    }
    
    public static bool Scp939Attack(Scp939 scp939, GameObject go)
    {
        if (go.TryGetComponent<BreakableWindow>(out var breakableWindow))
        {
            breakableWindow.Damage(50f, new ScpDamageHandler(scp939.Hub, 50f, DeathTranslations.Scp939), Vector3.zero);
            return true;
        }

        var victim = go?.GetSynapsePlayer();
        var scp = scp939.GetSynapsePlayer();

        if (scp == null | victim == null || victim.GodMode || victim.RoleType == RoleType.Spectator) return false;
        if (!Synapse3Extensions.GetHarmPermission(scp, victim)) return false;

        var ev = new Scp939AttackEvent(scp, victim, 50f, true);
        Synapse.Get<ScpEvents>().Scp939Attack.Raise(ev);
        if (!ev.Allow) return false;
        if (!victim.PlayerStats.DealDamage(new ScpDamageHandler(scp939.Hub, ev.Damage, DeathTranslations.Scp939)))
            return false;

        scp.ClassManager.RpcPlaceBlood(victim.transform.position, 0, 2f);
        //Dummies can't get Effects currently
        if (victim.PlayerType != PlayerType.Dummy)
            victim.PlayerEffectsController.EnableEffect<Amnesia>(3f, true);
        return true;
    }
    
    public static void Scp173Attack(Scp173 scp173, ReferenceHub hub)
    {
        var victim = hub?.GetSynapsePlayer();
        var scp = scp173.GetSynapsePlayer();

        if (scp == null | victim == null || victim.GodMode || victim.RoleType == RoleType.Spectator) return;
        if (!Synapse3Extensions.GetHarmPermission(scp, victim)) return;

        var damage = new ScpDamageHandler(scp173.Hub, DeathTranslations.Scp173);
        var ev = new Scp173AttackEvent(scp, victim, damage.Damage, true);
        Synapse.Get<ScpEvents>().Scp173Attack.Raise(ev);
        if(!ev.Allow) return;
        damage.Damage = ev.Damage;
        
        if (victim.GetEffect<Stained>().IsEnabled)
        {
            scp173.Shield.CurrentAmount = Mathf.Min(scp173.Shield.Limit,
                scp173.Shield.CurrentAmount + victim.GetStatBase<HealthStat>().CurValue);
        }
        
        if(!victim.PlayerStats.DealDamage(damage)) return;

        victim.ClassManager.RpcPlaceBlood(victim.Position, 0, 2.2f);
        Hitmarker.SendHitmarker(scp.Hub, 1.35f);
        new Scp173RpcMessage(scp.Hub, Scp173RpcMessage.Scp173RpcType.SnappedNecked).SendToAuthenticated();
    }
    
    public static void ObserveScp173(Scp173 scp173)
    {
        var count = scp173._observingPlayers.Count;
        var scp = scp173.GetSynapsePlayer();
        var config = Synapse.Get<SynapseConfigService>();

        foreach (var player in Synapse.Get<PlayerService>().Players)
        {
            if (player.RoleType == RoleType.Spectator || scp == player)
            {
                if (scp173._observingPlayers.Contains(player))
                    scp173._observingPlayers.Remove(player);
            }
            else
            {
                var pos = scp.Position;
                var room = RoomIdUtils.RoomAtPosition(pos);

                if (VisionInformation.GetVisionInformation(player, pos, -2f,
                        room?.Zone == FacilityZone.Surface ? 80f : 40f, false, false,
                        player.LocalCurrentRoomEffects).IsLooking &&
                    (!Physics.Linecast(pos + new Vector3(0f, 1.5f, 0f), player.CameraReference.position,
                        VisionInformation.VisionLayerMask) || !Physics.Linecast(pos + new Vector3(0f, -1f, 0f),
                        player.CameraReference.position, VisionInformation.VisionLayerMask)))
                {
                    var ev = new Scp173ObserveEvent(player,
                        player.Invisible < InvisibleMode.Ghost && !config.GamePlayConfiguration.CantObserve173.Contains(player.RoleID), scp);
                    Synapse.Get<ScpEvents>().Scp173Observed.Raise(ev);
                    if(!ev.Allow) continue;
                    
                    if (!scp173._observingPlayers.Contains(player))
                        scp173._observingPlayers.Add(player);
                }
                else if (scp173._observingPlayers.Contains(player))
                    scp173._observingPlayers.Remove(player);
            }
        }

        scp173._isObserved = scp173._observingPlayers.Count > 0 || scp173.StareAtDuration > 0f;

        if (count == scp173._observingPlayers.Count || !(scp173._blinkCooldownRemaining > 0f)) return;

        GameCore.Console.AddDebugLog("SCP173",
            $"Adjusting blink cooldown. Initial observers: {count}. New observers: {scp173._observingPlayers.Count}.",
            MessageImportance.LessImportant);
        GameCore.Console.AddDebugLog("SCP173", $"Current blink cooldown: {scp173._blinkCooldownRemaining}",
            MessageImportance.LeastImportant);

        scp173._blinkCooldownRemaining = Mathf.Max(0f,
            scp173._blinkCooldownRemaining + (scp173._observingPlayers.Count - count) * 0f); //Just don't ask why NorthWood is doing this

        GameCore.Console.AddDebugLog("SCP173", $"New blink cooldown: {scp173._blinkCooldownRemaining}",
            MessageImportance.LeastImportant);

        if (scp173._blinkCooldownRemaining <= 0f)
        {
            scp173.BlinkReady = true;
        }
    }
    
    public static void Scp0492Attack(Scp049_2PlayerScript script, GameObject gameObject)
    {
        if (!script._iawRateLimit.CanExecute() || !script.iAm049_2 || gameObject == null) return;

        if (script._remainingCooldown > 0f)
        {
            script._hub.characterClassManager.TargetConsolePrint(script.connectionToClient,
                "Zombie attack rejected (Z.1).", "gray");
            return;
        }
        script._remainingCooldown = script.attackCooldown - 0.09f;

        var scp = script.GetSynapsePlayer();
        var victim = gameObject.GetSynapsePlayer();
        if (scp == null || victim == null || victim.GodMode) return;
        if (!Synapse3Extensions.GetHarmPermission(scp, victim)) return;

        if (Vector3.Distance(scp.Position, victim.transform.position) > script.distance * 1.4f) return;
        
        var damage = new ScpDamageHandler(script._hub, script.damage, DeathTranslations.Zombie);
        var ev = new Scp0492AttackEvent(scp, victim, damage.Damage, true);
        Synapse.Get<ScpEvents>().Scp0492Attack.Raise(ev);
        if(!ev.Allow) return;
        
        var bloodPos = victim.Position;
        damage.Damage = ev.Damage;
        if(!victim.PlayerStats.DealDamage(damage)) return;
        Hitmarker.SendHitmarker(script.connectionToClient, 1f);
        scp.ClassManager.RpcPlaceBlood(bloodPos, 0, victim.RoleType == RoleType.Spectator ? 1.3f : 0.5f);
    }
    
    public static void Scp049AttackAndRevive(Scp049 scp049, byte action, GameObject go)
    {
        if(!scp049._interactRateLimit.CanExecute()) return;
        if (go == null) return;
        
        var scp = scp049.GetSynapsePlayer();
        if (scp == null) return;
        
        //Revive part when the Action is 1 or 2
        if (action is 1 or 2)
        {
            var ragdoll = go.GetComponent<Ragdoll>().GetSynapseRagdoll();
            var owner = ragdoll?.Owner;
            if (ragdoll == null || owner == null) return;

            var ev2 = new Scp049ReviveEvent(scp, owner, ragdoll, action == 2);

            if (!ev2.FinishRevive)
            {
                if (!ragdoll.CanBeReviveInTime)
                    ev2.Allow = false;
                
                if (!ragdoll.CanBeRevive)
                    ev2.Allow = false;

                var rigidbodies = ragdoll.Ragdoll.GetComponentsInChildren<Rigidbody>();
                var distance = false;
                for (int i = 0; i < rigidbodies.Length; i++)
                {
                    if (Vector3.Distance(rigidbodies[i].transform.position, scp.CameraReference.transform.position) <=
                        Scp049.ReviveDistance * 1.3f)
                    {
                        distance = true;
                        break;
                    }
                }
                if (!distance) ev2.Allow = false;
                
                Synapse.Get<ScpEvents>().Scp049Revive.Raise(ev2);
                if(!ev2.Allow) return;
                
                scp049._recallHubServer = owner;
                scp049._recallProgressServer = 0f;
                scp049._recallInProgressServer = true;   
            }
            else
            {
                if (!scp049._recallInProgressServer || owner != scp049._recallHubServer)
                    return;
                
                if (scp049._recallProgressServer < 0.85f)
                    ev2.Allow = false;

                if (owner.RoleType != RoleType.Spectator)
                    ev2.Allow = false;
                
                Synapse.Get<ScpEvents>().Scp049Revive.Raise(ev2);
                if(!ev2.Allow) return;

                owner.ClassManager.SetClassID(RoleType.Scp0492, CharacterClassManager.SpawnReason.Revived);
                ragdoll.Destroy();
                scp049._recallInProgressServer = false;
                scp049._recallHubServer = null;
                scp049._recallProgressServer = 0f;
            }
        }
        
        //Attack part when the Action is 0
        if(action != 0 || scp049.RemainingKillCooldown > 0f) return;
        
        
        var victim = go.GetSynapsePlayer();
        if (victim == null || victim.GodMode) return;
        if (!Synapse3Extensions.GetHarmPermission(scp, victim)) return;
        
        if(Vector3.Distance(go.transform.position, scp049.Hub.playerMovementSync.RealModelPosition) >= Scp049.AttackDistance * 1.25f) return;
        if(Physics.Linecast(scp049.Hub.playerMovementSync.RealModelPosition,go.transform.position,MicroHIDItem.WallMask)) return;

        var scpDamage = new ScpDamageHandler(scp049.Hub, DeathTranslations.Scp049);

        var ev = new Scp049AttackEvent(scp, victim, scpDamage.Damage, Scp049.KillCooldown, true);
        Synapse.Get<ScpEvents>().Scp049Attack.Raise(ev);
        
        if(!ev.Allow) return;
        scpDamage.Damage = ev.Damage;
        
        if(!victim.PlayerStats.DealDamage(scpDamage)) return;
        
        GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Sent 'death time' RPC", MessageImportance.LessImportant);
        scp.Hub.scpsController.RpcTransmit_Byte(0);
        scp049.RemainingKillCooldown = ev.Cooldown;
        return;
    }
}