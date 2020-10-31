using System;
using HarmonyLib;
using Mirror;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.AllowContain))]
    internal static class PlayerEnterFemurPatch
    {
        private static int FemurBrokePeople = 0;

        private static bool Prefix(CharacterClassManager __instance)
        {
            try
            {
                if (!NetworkServer.active) return false;
                if (!NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems) return false;

                foreach (var player in Server.Get.Players)
                {
                    if (!(Vector3.Distance(player.Position, __instance._lureSpj.transform.position) <
                          1.97f)) continue;
                    if (player.RoleType == RoleType.Spectator || player.GodMode) continue;
                    var allow = player.Team != Team.SCP;

                    var closeFemur = FemurBrokePeople + 1 >= Server.Get.Configs.SynapseConfiguration.RequiredForFemur;

                    SynapseController.Server.Events.Player.InvokePlayerEnterFemurEvent(player, ref allow, ref closeFemur);

                    if (!allow) return false;
                    player.Hurt(10000, DamageTypes.Lure);
                    FemurBrokePeople++;
                    if (closeFemur) __instance._lureSpj.SetState(true);
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