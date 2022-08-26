using System;
using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Items.MicroHID;
using MapGeneration;
using Neuron.Core.Logging;
using PlayableScps;
using PlayableScps.Messages;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using Utils.Networking;

namespace Synapse3.SynapseModule.Patches;

[Patches]
internal static class ScpPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp049), nameof(Scp049.BodyCmd_ByteAndGameObject))]
    public static bool Scp049Attack(Scp049 __instance, byte num, GameObject go)
    {
        try
        {
            return DecoratedScpPatches.Scp049AttackAndRevive(__instance, num, go);
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
}

internal static class DecoratedScpPatches
{
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
                        player.LocalCurrentRoomEffects, 0).IsLooking &&
                    (!Physics.Linecast(pos + new Vector3(0f, 1.5f, 0f), player.CameraReference.position,
                        VisionInformation.VisionLayerMask) || !Physics.Linecast(pos + new Vector3(0f, -1f, 0f),
                        player.CameraReference.position, VisionInformation.VisionLayerMask)))
                {
                    var ev = new ObserveScp173Event(player, !config.GamePlayConfiguration.CantObserve173.Contains(player.RoleID), scp);
                    Synapse.Get<ScpEvents>().ObserveScp173.Raise(ev);
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
            scp173._blinkCooldownRemaining + (scp173._observingPlayers.Count - count) * (0f)); //Just don't ask why NorthWood is doing this

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
    
    public static bool Scp049AttackAndRevive(Scp049 scp049, byte action, GameObject go)
    {
        if (go == null) return false;
        
        var scp = scp049.GetSynapsePlayer();
        if (scp == null) return false;
        
        //Revive part when the Action is 1 or 2
        if (action is 1 or 2)
        {
            var ragdoll = go.GetComponent<Ragdoll>().GetSynapseRagdoll();
            var owner = ragdoll?.Owner;

            if (ragdoll == null || owner == null) return false;

            var ev2 = new ReviveEvent(scp, owner, ragdoll, action == 2);
            Synapse.Get<ScpEvents>().Revive.Raise(ev2);
            return ev2.Allow;
        }
        
        //Attack part when the Action is 0
        if(action != 0 || scp049.RemainingKillCooldown > 0f) return false;
        
        if(!scp049._interactRateLimit.CanExecute()) return false;
        
        var victim = go.GetSynapsePlayer();
        if (victim == null || victim.GodMode) return false;
        if (!Synapse3Extensions.GetHarmPermission(scp, victim)) return false;
        
        if(Vector3.Distance(go.transform.position, scp049.Hub.playerMovementSync.RealModelPosition) >= Scp049.AttackDistance * 1.25f) return false;
        if(Physics.Linecast(scp049.Hub.playerMovementSync.RealModelPosition,go.transform.position,MicroHIDItem.WallMask)) return false;

        var scpDamage = new ScpDamageHandler(scp049.Hub, DeathTranslations.Scp049);

        var ev = new Scp049AttackEvent(scp, victim, scpDamage.Damage, Scp049.KillCooldown, true);
        Synapse.Get<ScpEvents>().Scp049Attack.Raise(ev);
        
        if(!ev.Allow) return false;
        scpDamage.Damage = ev.Damage;
        
        if(!victim.PlayerStats.DealDamage(scpDamage)) return false;
        
        GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Sent 'death time' RPC", MessageImportance.LessImportant);
        scp.Hub.scpsController.RpcTransmit_Byte(0);
        scp049.RemainingKillCooldown = ev.Cooldown;
        return false;
    }
}