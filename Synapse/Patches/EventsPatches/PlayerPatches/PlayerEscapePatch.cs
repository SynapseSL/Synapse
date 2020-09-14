using System;
using GameCore;
using Harmony;
using UnityEngine;

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
                var spawnRole = player.Role;
                var cuffedRole = RoleType.None;
                var allow = true;
                var isCuffed = false;

                //Cuff Check
                var flag = false;
                var component = __instance.GetComponent<Handcuffs>();
                if (component.CufferId >= 0)
                {
                    var component2 = component.GetCuffer(component.CufferId).GetComponent<CharacterClassManager>();

                    cuffedRole = component2.NetworkCurClass;
                    isCuffed = true;

                    if (ConfigFile.ServerConfig.GetBool("cuffed_escapee_change_team", true))
                    {
                        switch (__instance.CurClass)
                        {
                            case RoleType.Scientist when (component2.CurClass == RoleType.ChaosInsurgency || component2.CurClass == RoleType.ClassD):
                            case RoleType.ClassD when (component2.CurRole.team == Team.MTF || component2.CurClass == RoleType.Scientist):
                                flag = true;
                                break;
                        }
                    }
                }

                //TeamCheck
                var singleton = Respawning.RespawnTickets.Singleton;
                var team = __instance.CurRole.team;
                switch (team)
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
                SynapseController.Server.Events.Player.InvokePlayerEscapeEvent(player, ref spawnRole, cuffedRole, ref allow, isCuffed);

                if (!allow) return false;

                if (spawnRole == RoleType.None || spawnRole == __instance.NetworkCurClass) return false;
                __instance.SetPlayersClass(spawnRole, __instance.gameObject, false, true);
                switch (__instance.CurRole.team)
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