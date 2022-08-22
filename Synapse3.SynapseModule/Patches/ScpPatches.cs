using System;
using HarmonyLib;
using InventorySystem.Items.MicroHID;
using MapGeneration;
using Neuron.Core.Logging;
using PlayableScps;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using UnityEngine;

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
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp0492Attack Event Failed\n" + ex);
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
}

internal static class DecoratedScpPatches
{
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
        victim.PlayerStats.DealDamage(damage);
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