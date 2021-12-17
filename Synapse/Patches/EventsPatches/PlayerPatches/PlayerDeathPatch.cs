using System;
using System.Linq;
using HarmonyLib;
using PlayerStatsSystem;
using Synapse.Api;

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
                String handlerType = handler.GetType().ToString();

                Player Victim = __instance.GetPlayer();
                Player Attacker = null;
                float Damage = -1;
                ItemType Weapon = ItemType.None;
                bool allowed = true;

                switch (handlerType)
                {
                    case "PlayerStatsSystem.UniversalDamageHandler":
                        break;

                    case "PlayerStatsSystem.ScpDamageHandler":
                        ScpDamageHandler scpDamageHandler = (ScpDamageHandler) handler;

                        Attacker = scpDamageHandler.Attacker.Hub.GetPlayer();
                        break;

                    case "PlayerStatsSystem.FirearmDamageHandler":
                        FirearmDamageHandler firearmHandler = (FirearmDamageHandler) handler;

                        Attacker = firearmHandler.Attacker.Hub.GetPlayer();
                        Weapon = firearmHandler.WeaponType;
                        break;

                    case "PlayerStatsSystem.ExplosionDamageHandler":
                        ExplosionDamageHandler explosionDamageHandler = (ExplosionDamageHandler) handler;

                        Attacker = explosionDamageHandler.Attacker.Hub.GetPlayer();
                        break;

                    case "PlayerStatsSystem.CustomReasonDamageHandler":
                        break;

                    case "PlayerStatsSystem.MicroHidDamageHandler":
                        MicroHidDamageHandler microHidDamageHandler = (MicroHidDamageHandler) handler;

                        Attacker = microHidDamageHandler.Attacker.Hub.GetPlayer();
                        Weapon = ItemType.MicroHID;
                        break;

                    case "PlayerStatsSystem.Scp018DamageHandler":
                        Scp018DamageHandler scp018DamageHandler = (Scp018DamageHandler) handler;

                        Attacker = scp018DamageHandler.Attacker.Hub.GetPlayer();
                        Weapon = ItemType.SCP018;
                        break;

                    case "PlayerStatsSystem.Scp096DamageHandler":
                        Scp096DamageHandler scp096DamageHandler = (Scp096DamageHandler) handler;

                        Attacker = scp096DamageHandler.Attacker.Hub.GetPlayer();
                        break;
                }

                SynapseController.Server.Events.Player.InvokePlayerDeathEvent(Victim, Attacker, Weapon);
                
                foreach (var larry in Server.Get.Players.Where(x => x.Scp106Controller.PocketPlayers.Contains(Victim)))
                    larry.Scp106Controller.PocketPlayers.Remove(Victim);
                
                if (Victim.IsDummy)
                    Map.Get.Dummies.FirstOrDefault(x => x.Player == Victim)?.Destroy();
                
                return allowed;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerGeneratorInteract event failed!!\n{e}");
                return true;
            }
        }
    }
}