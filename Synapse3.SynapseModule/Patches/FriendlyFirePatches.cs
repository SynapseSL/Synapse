using System;
using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;
using Neuron.Core.Logging;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Patches;

[Patches]
[HarmonyPatch]
internal static class FriendlyFirePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HitboxIdentity), nameof(HitboxIdentity.CheckFriendlyFire), typeof(ReferenceHub), typeof(ReferenceHub), typeof(bool))]
    public static bool CheckFF(out bool __result, ReferenceHub attacker, ReferenceHub victim, bool ignoreConfig)
    {
        try
        {
            __result = Synapse3Extensions.GetHarmPermission(attacker, victim, ignoreConfig);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy 3 FF: Friendly Fire Patch failed\n" + ex);
            __result = true;
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FlashbangGrenade), nameof(FlashbangGrenade.PlayExplosionEffects))]
    public static bool FlashBangPatch(FlashbangGrenade __instance)
    {
        try
        {
            var time = __instance._blindingOverDistance.keys[__instance._blindingOverDistance.length - 1].time;
            var num = time * time;

            foreach (var player in Synapse.Get<PlayerService>().Players)
            {
                if((__instance.transform.position - player.Position).sqrMagnitude > num) continue;
                if(__instance.PreviousOwner.Hub == player) continue;
                if(!HitboxIdentity.CheckFriendlyFire(__instance.PreviousOwner.Hub,player)) continue;
                
                __instance.ProcessPlayer(player);
            }
            
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy 3 FF: FlashBang Patch failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HitboxIdentity), nameof(HitboxIdentity.Damage))]
    public static bool OnDamage(HitboxIdentity __instance, DamageHandlerBase handler)
    {
        try
        {
            if (handler is not AttackerDamageHandler aHandler) return true;

            var player = __instance.TargetHub.GetSynapsePlayer();
            var attacker = aHandler.Attacker.GetSynapsePlayer();
            return Synapse3Extensions.GetHarmPermission(attacker, player);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy 3 FF: On Hitbox Damage Patch failed\n" + ex);
            return true;
        }
    }
}