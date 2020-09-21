using System;
using Harmony;
using Mirror;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.AllowContain))]
    internal static class PlayerEnterFemurPatch
    {
        private static bool Prefix(CharacterClassManager __instance)
        {
            try
            {
                if (!NetworkServer.active) return false;
                if (!NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems) return false;

                foreach (var gameObject in PlayerManager.players)
                {
                    if (!(Vector3.Distance(gameObject.transform.position, __instance._lureSpj.transform.position) <
                          1.97f)) continue;
                    var component = gameObject.GetComponent<CharacterClassManager>();
                    var component2 = gameObject.GetComponent<PlayerStats>();
                    if (component.CurClass == RoleType.Spectator || component.GodMode) continue;
                    var allow = component.CurRole.team != Team.SCP;

                    //TODO: var closeFemur = FemurBrokePeople + 1 >= SynapseConfigs.RequiredForFemur;
                    var closeFemur = true;
                    var player = gameObject.GetPlayer();

                    SynapseController.Server.Events.Player.InvokePlayerEnterFemurEvent(player, ref allow, ref closeFemur);

                    if (!allow) return false;
                    component2.HurtPlayer(new PlayerStats.HitInfo(10000f, "WORLD", DamageTypes.Lure, 0), gameObject);
                    //TODO: FemurBrokePeople++;
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