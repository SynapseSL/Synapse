using System;
using HarmonyLib;
using PlayerStatsSystem;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.AllowContain))]
    internal static class PlayerEnterFemurPatch
    {
        private static int FemurBrokePeople = 0;

        [HarmonyPrefix]
        private static bool OnContain(CharacterClassManager __instance)
        {
            try
            {
                if (!NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems) return false;

                foreach (var player in Server.Get.Players)
                {
                    if (player.Hub.isDedicatedServer || !player.Hub.Ready) continue;
                    if (Vector3.Distance(player.Position, __instance._lureSpj.transform.position) >= 1.97f) continue;
                    if (player.RoleType == RoleType.Spectator || player.Team == Team.SCP) continue;
                    if (player.GodMode || !SynapseExtensions.CanHarmScp(player)) continue;

                    var allow = true;
                    var closeFemur = FemurBrokePeople + 1 >= Server.Get.Configs.synapseConfiguration.RequiredForFemur;

                    SynapseController.Server.Events.Player.InvokePlayerEnterFemurEvent(player, ref allow, ref closeFemur);

                    if (!allow) continue;
                    player.PlayerStats.DealDamage(new UniversalDamageHandler(10000, DeathTranslations.UsedAs106Bait));
                    FemurBrokePeople++;
                    if (closeFemur) __instance._lureSpj.SetState(__instance._lureSpj.allowContain, true);
                }

                return false;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerEnterFemur failed!!\n{e}");
                return true;
            }
        }
    }
}