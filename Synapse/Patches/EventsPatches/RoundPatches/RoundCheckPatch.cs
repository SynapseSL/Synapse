using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Logger = Synapse.Api.Logger;
using HarmonyLib;
using MEC;
using Synapse.Api;
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
            while(instance != null)
            {
                while (Map.Get.Round.RoundLock || !RoundSummary.RoundInProgress() || (instance._keepRoundOnOne && Server.Get.Players.Count < 2) || Map.Get.Round.RoundLength.TotalSeconds <= 3)
                    yield return Timing.WaitForOneFrame;

                var teams = new List<Team>();

                var customroles = new List<Synapse.Api.Roles.IRole>();

                var teamAmounts = 0;

                var leadingTeam = RoundSummary.LeadingTeam.Draw;

                var result = default(RoundSummary.SumInfo_ClassList);

                bool endround;

                foreach(var player in Server.Get.Players)
                {
                    if (player.CustomRole != null)
                        customroles.Add(player.CustomRole);

                    teams.Add(player.RealTeam);

                    switch (player.RealTeam)
                    {
                        case Team.SCP:
                            if (player.RoleID == (int)RoleType.Scp0492)
                                result.zombies++;
                            else
                                result.scps_except_zombies++;
                            break;

                        case Team.MTF:
                            result.mtf_and_guards++;
                            break;

                        case Team.CHI:
                            result.chaos_insurgents++;
                            break;

                        case Team.RSC:
                            result.scientists++;
                            break;

                        case Team.CDP:
                            result.class_ds++;
                            break;
                    }
                }

                if (teams.Contains(Team.MTF)) teamAmounts++;
                if (teams.Contains(Team.RSC)) teamAmounts++;
                if (teams.Contains(Team.CHI)) teamAmounts++;
                if (teams.Contains(Team.CDP)) teamAmounts++;
                if (teams.Contains(Team.SCP)) teamAmounts++;

                result.warhead_kills = Map.Get.Nuke.Detonated ? Map.Get.Nuke.NukeKills : -1;

                yield return Timing.WaitForOneFrame;

                result.time = (int)Time.realtimeSinceStartup;

                yield return Timing.WaitForOneFrame;

                RoundSummary.roundTime = result.time - instance.classlistStart.time;

                switch (teamAmounts)
                {
                    case 0:
                    case 1:
                        endround = true;
                        break;
                    case 2:
                        if (teams.Contains(Team.CHI) && teams.Contains(Team.SCP))
                            endround = Server.Get.Configs.synapseConfiguration.ChaosScpEnd;

                        else if (teams.Contains(Team.CHI) && teams.Contains(Team.CDP))
                            endround = true;

                        else if (teams.Contains(Team.MTF) && teams.Contains(Team.RSC))
                            endround = true;
                        else
                            endround = false;
                        break;
                    default:
                        endround = false;
                        break;
                }

                foreach (var role in customroles)
                    if (role.GetEnemys().Any(x => teams.Contains(x)))
                        endround = false;

                if (RoundSummary.escaped_ds + teams.Count(x => x == Team.CDP) > 0)
                {
                    if(teams.Contains(Team.SCP) || teams.Contains(Team.CHI) || teams.Contains(Team.CHI))
                        leadingTeam = RoundSummary.LeadingTeam.ChaosInsurgency;
                }
                else
                {
                    if (teams.Contains(Team.MTF) || teams.Contains(Team.RSC))
                    {
                        if (RoundSummary.escaped_scientists + teams.Count(x => x == Team.RSC) > 0)
                            leadingTeam = RoundSummary.LeadingTeam.FacilityForces;
                    }
                    else
                        leadingTeam = RoundSummary.LeadingTeam.Anomalies;
                }

                try
                {
                    Server.Get.Events.Round.InvokeRoundCheckEvent(ref endround, ref leadingTeam);
                }
                catch (Exception e)
                {
                    Logger.Get.Error($"Synapse-Event: RoundCheckEvent failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                }


                if (endround || Map.Get.Round.Forceend)
                {
                    instance._roundEnded = true;
                    Map.Get.Round.Forceend = false;
                    FriendlyFireConfig.PauseDetector = true;

                    var dpercentage = (float)instance.classlistStart.class_ds == 0 ? 0 : RoundSummary.escaped_ds + result.class_ds / instance.classlistStart.class_ds;
                    var spercentage = (float)instance.classlistStart.scientists == 0 ? 0 : RoundSummary.escaped_scientists + result.scientists / instance.classlistStart.scientists;
                    var text = $"Round finished! Anomalies: {teams.Where(x => x == Team.SCP).Count()} | Chaos: {teams.Where(x => x == Team.CHI || x == Team.CDP).Count()}" +
                        $" | Facility Forces: {teams.Where(x => x == Team.MTF || x == Team.RSC).Count()} | D escaped percentage: {dpercentage} | S escaped percentage : {spercentage}";
                    GameCore.Console.AddLog(text, Color.gray, false);
                    ServerLogs.AddLog(ServerLogs.Modules.Logger, text, ServerLogs.ServerLogType.GameEvent, false);

                    for (byte i = 0; i < 75; i++)
                        yield return 0f;

                    var timeToRoundRestart = Mathf.Clamp(GameCore.ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);

                    Map.Get.Round.ShowRoundSummary(result, leadingTeam);

                    for (int j = 0; j < 50 * timeToRoundRestart; j++)
                        yield return 0f;

                    Map.Get.Round.DimScreens();

                    for (byte i = 0; i < 50; i++)
                        yield return 0f;

                    Map.Get.Round.RestartRound();
                }
            }
        }
    }
}