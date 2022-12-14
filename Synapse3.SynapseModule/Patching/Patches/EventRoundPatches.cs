using System;
using System.Collections.Generic;
using System.Linq;
using GameCore;
using HarmonyLib;
using MEC;
using Neuron.Core.Meta;
using PlayerRoles;
using PluginAPI.Enums;
using PluginAPI.Events;
using RoundRestarting;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;
using UnityEngine;
using Console = GameCore.Console;

namespace Synapse3.SynapseModule.Patching.Patches;

[Automatic]
[SynapsePatch("RoundCheckEnd", PatchType.RoundEvent)]
public static class RoundCheckEndPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary._ProcessServerSideCode))]
    public static bool RoundSummaryOverride(RoundSummary __instance, ref IEnumerator<float> __result)
    {
        try
        {
            __result = DecoratedRoundMethods.ProcessServerSideCode(__instance);
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Sy3 Event: Round EndCheck Event Failed\n" + ex);
            return true;
        }
    }
}

/*TODO: Use the new RoleAssign namespace
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.))]
    public static bool SetRandomRoles(CharacterClassManager __instance, bool first)
    {
        try
        {
            DecoratedRoundMethods.OnSetRandomRoles(__instance, first);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Events: First Spawn Event failed:\n" + ex);
            return true;
        }
    }
    */

public static class DecoratedRoundMethods
{
    private static readonly PlayerService PlayerService;
    private static readonly RoundEvents RoundEvents;
    private static readonly SynapseConfigService ConfigService;

    static DecoratedRoundMethods()
    {
        PlayerService = Synapse.Get<PlayerService>();
        RoundEvents = Synapse.Get<RoundEvents>();
        ConfigService = Synapse.Get<SynapseConfigService>();
    }
    
    public static IEnumerator<float> ProcessServerSideCode(RoundSummary summary)
    {
        var time = Time.unscaledTime;
        while (summary != null)
        {
            yield return Timing.WaitForSeconds(2.5f);
            if (RoundSummary.RoundLock) continue;
            if (summary.KeepRoundOnOne && PlayerService.Players.Count == 1) continue;
            if (RoundSummary.RoundInProgress() && Time.unscaledTime - time < 15f) continue;

            var customRoles = new List<ISynapseRole>();
            var livingTeams = new List<uint>();
            var roundData = default(RoundSummary.SumInfo_ClassList);
            foreach (var player in PlayerService.Players)
            {
                if (!livingTeams.Contains(player.TeamID))
                    livingTeams.Add(player.TeamID);
                
                if (player.HasCustomRole)
                {
                    customRoles.Add(player.CustomRole);
                    continue;
                }
                
                switch (player.Team)
                {
                    case Team.SCPs:
                        if (player.RoleType == RoleTypeId.Scp0492)
                            roundData.zombies++;
                        else
                            roundData.scps_except_zombies++;
                        break;
                    
                    case Team.FoundationForces:
                        roundData.mtf_and_guards++;
                        break;
                    
                    case Team.ChaosInsurgency:
                        roundData.chaos_insurgents++;
                        break;
                    
                    case Team.Scientists:
                        roundData.scientists++;
                        break;
                    
                    case Team.ClassD:
                        roundData.class_ds++;
                        break;
                }

                yield return Timing.WaitForOneFrame;

                roundData.warhead_kills =
                    AlphaWarheadController.Detonated ? AlphaWarheadController.Singleton.WarheadKills : -1;
                
                yield return Timing.WaitForOneFrame;

                var numberOfFoundationStaff = roundData.mtf_and_guards + roundData.scientists;
                var numberOfNonFoundationStaff = roundData.chaos_insurgents + roundData.class_ds;
                var numberOfScps = roundData.scps_except_zombies + roundData.zombies;
                var escapedDPersonnel = roundData.class_ds + RoundSummary.EscapedClassD;
                var escapedScientist = roundData.scientists + RoundSummary.EscapedScientists;
                var mtfAlive = numberOfFoundationStaff > 0;
                var chaosAlive = numberOfNonFoundationStaff > 0;
                var scpAlive = numberOfScps > 0;

                RoundSummary.SurvivingSCPs = roundData.scps_except_zombies;

                var escapedDPersonnelPercentage = summary.classlistStart.class_ds == 0
                    ? 0
                    : escapedDPersonnel / summary.classlistStart.class_ds;
                var escapedScientistPercentage = summary.classlistStart.scientists == 0
                    ? 1
                    : escapedScientist / summary.classlistStart.scientists;

                //This checks for the single case where 2 Teams are still alive (SCP and Chaos) but the round should end
                if (!ConfigService.GamePlayConfiguration.ChaosAndScpEnemy && roundData.class_ds <= 0 && numberOfFoundationStaff <= 0)
                {
                    summary._roundEnded = true;
                }
                else
                {
                    var amountOfFactions = 0;
                    
                    if (mtfAlive)
                        amountOfFactions++;
                    
                    if (chaosAlive)
                        amountOfFactions++;

                    if (scpAlive)
                        amountOfFactions++;

                    summary._roundEnded = amountOfFactions <= 1;
                }

                if (summary._roundEnded)
                {
                    foreach (var customRole in customRoles)
                    {
                        if (customRole.GetEnemiesID().Any(x => livingTeams.Contains(customRole.Attribute.TeamId)))
                        {
                            summary._roundEnded = false;
                            break;
                        }
                    }
                }
                
                var leadingTeam = RoundSummary.LeadingTeam.Draw;

                if (mtfAlive)
                {
                    leadingTeam = escapedScientist >= escapedDPersonnel
                        ? RoundSummary.LeadingTeam.FacilityForces
                        : RoundSummary.LeadingTeam.Draw;
                }
                else if (scpAlive)
                {
                    leadingTeam = escapedDPersonnel > RoundSummary.SurvivingSCPs
                        ?
                        RoundSummary.LeadingTeam.ChaosInsurgency
                        : RoundSummary.SurvivingSCPs > escapedScientist
                            ? RoundSummary.LeadingTeam.Anomalies
                            : RoundSummary.LeadingTeam.Draw;
                }
                else if (chaosAlive)
                {
                    leadingTeam = escapedDPersonnel >= escapedScientist
                        ? RoundSummary.LeadingTeam.ChaosInsurgency
                        : RoundSummary.LeadingTeam.Draw;
                }

                var ev = new RoundCheckEndEvent()
                {
                    EndRound = summary._roundEnded,
                    WinningTeam = leadingTeam
                };
                RoundEvents.CheckEnd.Raise(ev);
                summary._roundEnded = ev.EndRound;
                
                if(!summary._roundEnded) continue;

                EventManager.ExecuteEvent(ServerEventType.RoundEnd, Array.Empty<object>());
                FriendlyFireConfig.PauseDetector = true;

                var log =
                    $"Round finished! Anomalies: {numberOfScps} | Chaos: {numberOfNonFoundationStaff} | Facility Forces: {numberOfFoundationStaff} | D escaped percentage: {escapedDPersonnelPercentage} | S escaped percentage: {escapedScientistPercentage}";
                Console.AddLog(log, Color.gray);
                ServerLogs.AddLog(ServerLogs.Modules.Logger, log, ServerLogs.ServerLogType.GameEvent);

                yield return Timing.WaitForSeconds(1.5f);
                var restartTime = Mathf.Clamp(ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);
                if (summary != null)
                {
                    summary.RpcShowRoundSummary(summary.classlistStart, roundData, leadingTeam, escapedDPersonnel,
                        escapedScientist, RoundSummary.KilledBySCPs, restartTime,
                        (int)RoundStart.RoundLength.TotalSeconds);
                }
                yield return Timing.WaitForSeconds(restartTime - 1f);
                summary.RpcDimScreen();
                yield return Timing.WaitForSeconds(1f);
                RoundRestart.InitiateRoundRestart();
            }
        }
    }
}