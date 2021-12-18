using System;
using System.Linq;
using HarmonyLib;
using PlayerStatsSystem;
using Synapse.Api;
using Synapse.Api.Items;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.KillPlayer))]
    internal class PlayerDeathPatch
    {
        [HarmonyPrefix]
        private static bool OnDeath(PlayerStats __instance, DamageHandlerBase handler)
        {
            try
            {
                Player Victim = __instance.GetPlayer();
                Player Attacker;
                float Damage;
                SynapseItem Weapon;
                ItemType WeaponType;

                handler.Analyze(out Attacker, out Weapon, out WeaponType, out Damage);

                SynapseController.Server.Events.Player.InvokePlayerDeathEvent(Victim, Attacker, Damage, WeaponType, Weapon);
                
                foreach (var larry in Server.Get.Players.Where(x => x.Scp106Controller.PocketPlayers.Contains(Victim)))
                    larry.Scp106Controller.PocketPlayers.Remove(Victim);
                
                if (Victim.IsDummy)
                    Map.Get.Dummies.FirstOrDefault(x => x.Player == Victim)?.Destroy();
                
                return true;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerGeneratorInteract event failed!!\n{e}");
                return true;
            }
        }
    }
}