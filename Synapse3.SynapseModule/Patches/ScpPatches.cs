using System;
using HarmonyLib;
using InventorySystem.Items.MicroHID;
using Neuron.Core.Logging;
using PlayableScps;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Events;
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
            DecoratedScpPatches.Scp049Attack(__instance, num, go);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp049Attack Event Failed\n" + ex);
            return true;
        }
    }
}

internal static class DecoratedScpPatches
{
    public static void Scp049Attack(PlayableScps.Scp049 scp049, byte action, GameObject go)
    {
        if(action != 0 || go == null || scp049.RemainingKillCooldown > 0f) return;
        if(!scp049._interactRateLimit.CanExecute()) return;

        var scp = scp049.GetSynapsePlayer();
        var victim = go.GetSynapsePlayer();
        if(scp == null || victim == null) return;
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
    }
}