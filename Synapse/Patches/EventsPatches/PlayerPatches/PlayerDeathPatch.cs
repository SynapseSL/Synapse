using HarmonyLib;
using PlayerStatsSystem;
using Synapse.Api;
using Synapse.Api.Enum;
using System;
using System.Linq;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.KillPlayer))]
    internal class PlayerDeathPatch
    {
        [HarmonyPrefix]
        private static bool OnDeath(PlayerStats __instance, DamageHandlerBase handler,out bool __state)
        {
            try
            {
                var standardhandler = handler as StandardDamageHandler;
                var type = handler.GetDamageType();
                var victim = __instance.GetPlayer();
                var attacker = handler is AttackerDamageHandler ahandler ? ahandler.Attacker.GetPlayer() : null;

                if (type == DamageType.PocketDecay)
                    attacker = Server.Get.Players.FirstOrDefault(x => x.Scp106Controller.PocketPlayers.Contains(victim));

                SynapseController.Server.Events.Player.InvokePlayerDeathEvent(victim, attacker, type, out var allow);

                if (!allow) victim.Health = 1;
                if (allow) victim.DeathPosition = victim.Position;
                __state = allow;
                return allow;
            }
            catch (Exception e)
            {
                __state = true;
                Logger.Get.Error($"Synapse-Event: PlayerDeath event failed!!\n{e}");
                return true;
            }
        }

        [HarmonyPostfix]
        private static void OnPostDeath(PlayerStats __instance,bool __state)
        {
            if (__state)
            {
                var victim = __instance.GetPlayer();

                foreach (var larry in Server.Get.Players.Where(x => x.Scp106Controller.PocketPlayers.Contains(victim)))
                    larry.Scp106Controller.PocketPlayers.Remove(victim);

                if (victim.IsDummy)
                    Map.Get.Dummies.FirstOrDefault(x => x.Player == victim)?.Destroy();

                foreach (var larry in Server.Get.Players.Where(x => x.Scp106Controller.PocketPlayers.Contains(victim)))
                    larry.Scp106Controller.PocketPlayers.Remove(victim);

                if (victim.IsDummy)
                    Map.Get.Dummies.FirstOrDefault(x => x.Player == victim)?.Destroy();

                victim.CustomRole = null;
            }
        }
    }
}