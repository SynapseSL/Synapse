using System;
using System.Linq;
using HarmonyLib;
using MEC;
using Neuron.Core.Logging;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;

namespace Synapse3.SynapseModule.Patches.PlayerPatches;

[Patches]
[HarmonyPatch]
internal static class DeathPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.KillPlayer))]
    public static bool PreDeath(PlayerStats __instance, ref DamageHandlerBase handler)
    {
        try
        {
            var damage =  (handler as StandardDamageHandler)?.Damage ?? 0;
            
            var player = __instance.GetSynapsePlayer();
            var attacker = (handler as AttackerDamageHandler)?.Attacker.GetSynapsePlayer();
            var damageType = handler.GetDamageType();

            if (damageType == DamageType.PocketDecay)
                attacker = Synapse.Get<PlayerService>().Players
                    .FirstOrDefault(x => x.ScpController.Scp106.PlayersInPocket.Contains(player));

            var ev = new DeathEvent(player, true, attacker, damageType, damage, null);
            Synapse.Get<PlayerEvents>().Death.Raise(ev);

            if (!ev.Allow)
            {
                player.Health = 1;
                return false;
            }
            
            if (ev.DeathMessage != null)
                handler = new CustomReasonDamageHandler(ev.DeathMessage);

            player.DeathPosition = player.Position;

            return true;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Player Death Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.KillPlayer))]
    public static void PostDeath(PlayerStats __instance, DamageHandlerBase handler, bool __runOriginal)
    {
        try
        {
            if (!__runOriginal) return;
            var victim = __instance.GetSynapsePlayer();
            var service = Synapse.Get<PlayerService>();

            foreach (var larry in service.GetPlayers(x => x.ScpController.Scp106.PlayersInPocket.Contains(victim)))
            {
                larry.ScpController.Scp106.PlayersInPocket.Remove(victim);
            }
                
            if (victim.PlayerType == PlayerType.Dummy)
            {
                Timing.CallDelayed(Timing.WaitForOneFrame, () => (victim as DummyPlayer)?.SynapseDummy.Destroy());
            }

            victim.RemoveCustomRole(DespawnReason.Death);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Player Death(Post) Event failed\n" + ex);
        }
    }
}