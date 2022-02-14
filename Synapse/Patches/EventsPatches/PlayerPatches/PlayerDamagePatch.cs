using HarmonyLib;
using PlayerStatsSystem;
using Synapse.Api.Enum;
using System;
using System.Linq;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.DealDamage))]
    internal static class PlayerDamagePatch
    {
        [HarmonyPrefix]
        private static bool DealDamagePatch(PlayerStats __instance, DamageHandlerBase handler)
        {
            try
            {
                if (!__instance._hub.characterClassManager.IsAlive
                    || __instance._hub.characterClassManager.GodMode)
                    return false;

                var standardhandler = handler as StandardDamageHandler;
                var type = handler.GetDamageType();
                var victim = __instance.GetPlayer();
                var attacker = handler is AttackerDamageHandler ahandler ? ahandler.Attacker.GetPlayer() : null;
                var damage = standardhandler.Damage;

                if (type == DamageType.PocketDecay)
                {
                    attacker = Server.Get.Players.FirstOrDefault(x => x.Scp106Controller.PocketPlayers.Contains(victim));
                    if (attacker != null && !SynapseExtensions.GetHarmPermission(attacker, victim)) return false;
                }

                SynapseController.Server.Events.Player.InvokePlayerDamageEvent(victim, attacker, ref damage, type, out var allow);
                standardhandler.Damage = damage;
                
                return allow;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerDamage event failed!!\n{e}");
                return true;
            }
        }
    }
}