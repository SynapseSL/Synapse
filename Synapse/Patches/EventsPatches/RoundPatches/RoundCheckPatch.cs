using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using GameCore;
using Harmony;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.RoundPatches
{
    [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.Start))]
    internal static class CheckRoundEndPatch
    {
        private static readonly MethodInfo
            CustomProcess = SymbolExtensions.GetMethodInfo(() => ProcessServerSide(null));
        
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            var codes = new List<CodeInstruction>(instr);

            foreach (var code in codes.Select((x,i) => new {Value =x, Index = i }))
            {
                if (code.Value.opcode != OpCodes.Call) continue;

                if (code.Value.operand != null && code.Value.operand is MethodBase methodBase &&
                    methodBase.Name == nameof(RoundSummary._ProcessServerSideCode))
                {
                    codes[code.Index].operand = CustomProcess;
                }
            }

            return codes.AsEnumerable();
        }

        public static IEnumerator<float> ProcessServerSide(RoundSummary instance)
        {
            var roundSummary = instance;

            while (roundSummary != null)
            {
                while (RoundSummary.RoundLock || !RoundSummary.RoundInProgress() ||
                       roundSummary._keepRoundOnOne && PlayerManager.players.Count < 2) yield return 0.0f;
                
                var newList = new RoundSummary.SumInfo_ClassList();

                foreach (var chrClassManager in PlayerManager.players.Where(gameObject => gameObject != null).Select(gameObject => gameObject.GetComponent<CharacterClassManager>()).Where(chrClassManager => chrClassManager.Classes.CheckBounds(chrClassManager.CurClass)))
                {
                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                    switch (chrClassManager.Classes.SafeGet(chrClassManager.CurClass).team)
                    {
                        case Team.SCP:
                            if (chrClassManager.CurClass == RoleType.Scp0492)
                            {
                                newList.zombies++;
                                continue;
                            }

                            newList.scps_except_zombies++;
                            continue;
                        case Team.MTF:
                            newList.mtf_and_guards++;
                            continue;
                        case Team.CHI:
                            newList.chaos_insurgents++;
                            continue;
                        case Team.RSC:
                            newList.scientists++;
                            continue;
                        case Team.CDP:
                            newList.class_ds++;
                            continue;
                        default:
                            continue;
                    }
                }

                newList.warhead_kills =
                    AlphaWarheadController.Host.detonated ? AlphaWarheadController.Host.warheadKills : -1;

                yield return float.NegativeInfinity;
                newList.time = (int) Time.realtimeSinceStartup;
                yield return float.NegativeInfinity;

                RoundSummary.roundTime = newList.time - roundSummary.classlistStart.time;

                var mtfSum = newList.mtf_and_guards + newList.scientists;
                var chaosSum = newList.chaos_insurgents + newList.class_ds;
                var scpSum = newList.scps_except_zombies + newList.zombies;

                var escapedDs = (float)(roundSummary.classlistStart.class_ds == 0 ? 0 : (RoundSummary.escaped_ds + newList.class_ds) / roundSummary.classlistStart.class_ds);
                var escapedScientists = (float)(roundSummary.classlistStart.scientists == 0 ? 1 : (RoundSummary.escaped_scientists + newList.scientists) / roundSummary.classlistStart.scientists);

                var allow = true;
                var forceEnd = false;
                var teamChanged = false;
                var team = RoundSummary.LeadingTeam.Draw;

                try
                {
                    //Code from Synapse for CustomRoles and ChaosScpEnd
                    List<Team> teams = Server.Get.Players.Select(x => x.RealTeam).ToList();

                    var teamAmounts = 0;
                    if (teams.Contains(Team.MTF)) teamAmounts++;
                    if (teams.Contains(Team.RSC)) teamAmounts++;
                    if (teams.Contains(Team.CHI)) teamAmounts++;
                    if (teams.Contains(Team.CDP)) teamAmounts++;
                    if (teams.Contains(Team.SCP)) teamAmounts++;

                    var roundEnd = teamAmounts < 2;
                    if (teamAmounts == 2)
                    {
                        if (teams.Contains(Team.CHI) && teams.Contains(Team.SCP))
                            roundEnd = Server.Get.Configs.SynapseConfiguration.ChaosScpEnd;

                        if (teams.Contains(Team.CHI) && teams.Contains(Team.CDP))
                            roundEnd = true;

                        if (teams.Contains(Team.MTF) && teams.Contains(Team.RSC))
                            roundEnd = true;
                    }

                    foreach (var role in Server.Get.GetPlayers(x => x.CustomRole != null).Select(x => x.CustomRole))
                        if (role.GetEnemys().Any(t => teams.Contains(t)))
                            roundEnd = false;

                    if (!roundEnd) allow = false;
                    else
                    {
                        forceEnd = true;

                        if (RoundSummary.escaped_ds + teams.Count(x => x == Team.CDP) > 0)
                        {
                            if (!teams.Contains(Team.SCP) && !teams.Contains(Team.CHI))
                                team = RoundSummary.LeadingTeam.Draw;
                            else
                                team = RoundSummary.LeadingTeam.ChaosInsurgency;
                        }
                        else
                        {
                            if (teams.Contains(Team.MTF) || teams.Contains(Team.RSC))
                            {
                                team = RoundSummary.escaped_scientists + teams.Count(x => x == Team.RSC) > 0 ? RoundSummary.LeadingTeam.FacilityForces : RoundSummary.LeadingTeam.Draw;
                            }
                            else team = RoundSummary.LeadingTeam.Anomalies;
                        }
                    }

                    SynapseController.Server.Events.Round.InvokeRoundCheckEvent(ref forceEnd, ref allow, ref team, ref teamChanged);
                }
                catch (Exception e)
                {
                    SynapseController.Server.Logger.Error($"Synapse-Event: CheckRound failed!!\n{e}");
                    continue;
                }

                if (forceEnd) roundSummary._roundEnded = true;
                
                if(!allow) continue;

                if (newList.class_ds == 0 && mtfSum == 0)
                {
                    roundSummary._roundEnded = true;
                }
                
                else if (mtfSum == 0 && Respawning.RespawnTickets.Singleton.GetAvailableTickets(Respawning.SpawnableTeamType.NineTailedFox) == 0)
                {
                    roundSummary._roundEnded = true;
                }

                else
                {
                    //Okay. SCP hat hier einfach wirklich nur Staub gefressen oder so.
                    var checkVar = 0;

                    if (mtfSum > 0) checkVar++;
                    if (chaosSum > 0) checkVar++;
                    if (scpSum > 0) checkVar++;

                    if (checkVar <= 1) roundSummary._roundEnded = true;
                }
                
                
                if (!roundSummary._roundEnded) continue;
                var leadingTeam = RoundSummary.LeadingTeam.Draw;

                if (mtfSum > 0)
                {
                    if (RoundSummary.escaped_ds == 0 && RoundSummary.escaped_scientists != 0)
                        leadingTeam = RoundSummary.LeadingTeam.FacilityForces;
                }
                else
                    leadingTeam = RoundSummary.escaped_ds != 0
                        ? RoundSummary.LeadingTeam.ChaosInsurgency
                        : RoundSummary.LeadingTeam.Anomalies;

                if (teamChanged) leadingTeam = team;

                var text = $"Round finished! Anomalies:{scpSum} | Chaos: {chaosSum} | Facility Forces: {mtfSum} | D escaped percentage: {escapedDs} | S escaped percentage: {escapedScientists}";
                
                GameCore.Console.AddLog(text, Color.gray);
                ServerLogs.AddLog(ServerLogs.Modules.Logger, text, ServerLogs.ServerLogType.GameEvent);
                
                for (byte i = 0; i < 75; i += 1)
                {
                    yield return 0f;
                }
                var timeToRoundRestart = Mathf.Clamp(ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);
                if (roundSummary != null)
                {
                    roundSummary.RpcShowRoundSummary(roundSummary.classlistStart, newList, leadingTeam, RoundSummary.escaped_ds, RoundSummary.escaped_scientists, RoundSummary.kills_by_scp, timeToRoundRestart);
                }
                int num7;
                for (var j = 0; j < 50 * (timeToRoundRestart - 1); j = num7 + 1)
                {
                    yield return 0f;
                    num7 = j;
                }
                roundSummary.RpcDimScreen();
                for (byte i = 0; i < 50; i += 1)
                {
                    yield return 0f;
                }
                PlayerManager.localPlayer.GetComponent<PlayerStats>().Roundrestart();
            }
        }
    }
}