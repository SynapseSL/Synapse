using HarmonyLib;
using PlayerStatsSystem;
using Synapse.Api.Enum;
using System;
using System.Linq;
using System.Reflection;
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
                var allow = attacker != null ? SynapseExtensions.GetHarmPermission(attacker, victim) : true;

                if (type == DamageType.PocketDecay)
                {
                    attacker = Server.Get.Players.FirstOrDefault(x => x.Scp106Controller.PocketPlayers.Contains(victim));
                    if (attacker != null && !SynapseExtensions.GetHarmPermission(attacker, victim)) return false;
                }

                SynapseController.Server.Events.Player.InvokePlayerDamageEvent(victim, attacker, ref damage, type, ref allow);
                standardhandler.Damage = damage;
                
                return allow;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerDamage event failed!!\n{e}");
                return true;
            }
        }

        private static void CallEvent(this PlayerStats source, string eventName, object[] parameters)
        {
            var eventsField = typeof(PlayerStats).GetField(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (eventsField != null)
            {
                object eventHandlerList = eventsField.GetValue(source);
                if (eventHandlerList != null)
                {
                    var my_event_invoke = eventHandlerList.GetType().GetMethod("Invoke");
                    if (my_event_invoke != null)
                    {
                        my_event_invoke.Invoke(eventHandlerList, parameters);
                    }
                }
                else Server.Get.Logger.Error("Synapse-Event: PlayerDamage failed!! \n eventHandlerList null");
            }
            else
            {
                Server.Get.Logger.Error("Synapse-Event: PlayerDamage failed!! \n eventsField null");
            }
        }


        private static void CallStaticEvent(string eventName, object[] parameters)
        {
            var eventsField = typeof(PlayerStats).GetField(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (eventsField != null)
            {
                object eventHandlerList = eventsField.GetValue(null);
                if (eventHandlerList != null)
                {
                    var my_event_invoke = eventHandlerList.GetType().GetMethod("Invoke");
                    if (my_event_invoke != null)
                    {
                        my_event_invoke.Invoke(eventHandlerList, parameters);
                    }
                }
                else Server.Get.Logger.Error("Synapse-Event: PlayerDamage failed!! \n eventHandlerList null");
            }
            else
            {
                Server.Get.Logger.Error("Synapse-Event: PlayerDamage failed!! \n eventsField null");
            }
        }

    }
}