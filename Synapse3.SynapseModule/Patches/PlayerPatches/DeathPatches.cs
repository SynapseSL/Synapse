using HarmonyLib;
using MEC;
using Neuron.Core.Logging;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;
using System;
using System.Linq;
using System.Reflection;

namespace Synapse3.SynapseModule.Patches.PlayerPatches;

[Patches]
[HarmonyPatch]
internal static class DeathPatches
{
    static SynapseTranslation _translation;
    static PlayerService _player;

    static DeathPatches()
    {
        _translation = Synapse.Get<SynapseConfigService>().Translation;
        _player = Synapse.Get<PlayerService>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.KillPlayer))]
    public static bool Death(PlayerStats __instance, DamageHandlerBase handler)
    {
        try
        {
            //--Synapse Event--
            var damage = (handler as StandardDamageHandler)?.Damage ?? 0;

            var victim = __instance.GetSynapsePlayer();
            var attacker = (handler as AttackerDamageHandler)?.Attacker.GetSynapsePlayer();
            var damageType = handler.GetDamageType();

            if (damageType == DamageType.PocketDecay)
                attacker = _player.Players.FirstOrDefault(x => x.ScpController.Scp106.PlayersInPocket.Contains(victim));

            string playerMsg = null;

            if (attacker?.CustomRole != null)
            {
                var translation = victim.GetTranslation(_translation).DeathMessage.Replace("\\n", "\n");
                playerMsg = string.Format(translation, attacker.DisplayName, attacker.RoleName);
            }

            var ev = new DeathEvent(victim, true, attacker, damageType, damage, playerMsg, null);
            Synapse.Get<PlayerEvents>().Death.Raise(ev);

            if (!ev.Allow)
            {
                victim.Health = 1;
                return false;
            }

            victim.DeathPosition = victim.Position;
            
            playerMsg = ev.DeathMessage;
            var ragdollInfo = ev.RagdollInfo;

            //--Vanila Stuff rework--
            if (ragdollInfo != null)
                Ragdoll.ServerSpawnRagdoll(victim, new CustomReasonDamageHandler(ragdollInfo));
            else
                Ragdoll.ServerSpawnRagdoll(victim, handler);

            if (playerMsg != null)
                __instance.TargetReceiveSpecificDeathReason(new CustomReasonDamageHandler(playerMsg));
            else if (attacker != null)//I replace the NickName by display name, I find it more consistent for Synapse
                __instance.TargetReceiveAttackerDeathReason(attacker.DisplayName, attacker.RoleType);
            else
                __instance.TargetReceiveSpecificDeathReason(handler);

            victim.Inventory.DropEverything();

            var classManager = victim.ClassManager;
            classManager.SetClassID(RoleType.Spectator, CharacterClassManager.SpawnReason.Died);
            classManager.TargetConsolePrint(classManager.connectionToClient, "You died. Reason: " + handler.ServerLogsText, "yellow");

            //--Synapse API--
            foreach (var larry in _player.Players)
            {
                var playerPocket = larry.ScpController.Scp106.PlayersInPocket;
                if (playerPocket.Contains(victim))
                    playerPocket.Remove(victim);
            }

            if (victim.PlayerType == PlayerType.Dummy)
            {
                Timing.CallDelayed(Timing.WaitForOneFrame, () => (victim as DummyPlayer)?.SynapseDummy.Destroy());
            }

            victim.RemoveCustomRole(DeSpawnReason.Death);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Player Death Event failed\n" + ex);
            return true;
        }
    }
}