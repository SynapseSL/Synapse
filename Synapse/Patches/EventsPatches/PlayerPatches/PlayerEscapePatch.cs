using System;
using GameCore;
using Harmony;
using UnityEngine;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.CallCmdRegisterEscape))]
    internal static class PlayerEscapePatch
    {
        private static bool Prefix(CharacterClassManager __instance)
        {
            try
            {
                //Ratelimit Check
                if (!__instance._interactRateLimit.CanExecute()) return false;

                //Position Check
                if (Vector3.Distance(__instance.transform.position, __instance.GetComponent<Escape>().worldPosition) >=
                    Escape.radius * 2) return false;

                //Event vars
                var player = __instance.GetPlayer();
                var spawnRole = player.RoleType;
                var cufferRole = RoleType.None;
                var allow = true;
                var isCuffed = false;

                //Cuff Check
                var flag = false;
                if (player.Cuffer != null)
                {
                    cufferRole = player.Cuffer.RoleType;
                    isCuffed = true;

                    if (ConfigFile.ServerConfig.GetBool("cuffed_escapee_change_team", true))
                        switch (player.RoleType)
                        {
                            case RoleType.Scientist when (cufferRole == RoleType.ChaosInsurgency || cufferRole == RoleType.ClassD):
                            case RoleType.ClassD when (player.Cuffer.Team == Team.MTF || cufferRole == RoleType.Scientist):
                                flag = true;
                                break;
                        }
                }

                //TeamCheck
                var singleton = Respawning.RespawnTickets.Singleton;
                switch (player.Team)
                {
                    case Team.CDP when flag:
                        spawnRole = RoleType.NtfCadet;
                        break;
                    case Team.CDP:
                    case Team.RSC when flag:
                        spawnRole = RoleType.ChaosInsurgency;
                        break;
                    case Team.RSC:
                        spawnRole = RoleType.NtfScientist;
                        break;
                }

                //PlayerEscapeEvent
                SynapseController.Server.Events.Player.InvokePlayerEscapeEvent(player, ref spawnRole, cufferRole, ref allow, isCuffed);

                if (!allow) return false;

                if (spawnRole == RoleType.None || spawnRole == player.RoleType) return false;
                player.ClassManager.SetPlayersClass(spawnRole, player.gameObject, false, true);
                switch (player.Team)
                {
                    case Team.MTF:
                        RoundSummary.escaped_scientists++;
                        singleton.GrantTickets(Respawning.SpawnableTeamType.NineTailedFox, ConfigFile.ServerConfig.GetInt("respawn_tickets_mtf_classd_cuffed_count", 1));
                        break;
                    case Team.CHI:
                        RoundSummary.escaped_ds++;
                        singleton.GrantTickets(Respawning.SpawnableTeamType.ChaosInsurgency, ConfigFile.ServerConfig.GetInt("respawn_tickets_ci_classd_count", 1));
                        break;
                }

                return false;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerEscape failed!!\n{e}");
                return true;
            }
        }
    }
}