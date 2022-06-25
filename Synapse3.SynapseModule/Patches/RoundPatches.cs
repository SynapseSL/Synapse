using System;
using System.Collections.Generic;
using GameCore;
using HarmonyLib;
using MEC;
using Neuron.Core.Logging;
using RoundRestarting;
using Synapse3.SynapseModule.Events;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace Synapse3.SynapseModule.Patches;

[Patches]
internal static class RoundPatches
{
    private static bool _firstTime = true;
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SpawnpointManager), nameof(SpawnpointManager.FillSpawnPoints))]
    public static void RoundWaitingPatch()
    {
        try
        {
            var ev = new RoundWaitingEvent
            {
                FirstTime = _firstTime
            };
            Synapse.Get<RoundEvents>().RoundWaiting.Raise(ev);
            _firstTime = false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Round Waiting Event Failed\n" + ex);
        }
    }

    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ServerLogs), nameof(ServerLogs.AddLog))]
    public static void RoundEndPatch(ref ServerLogs.Modules module, ref string msg, ref ServerLogs.ServerLogType type, ref bool init)
    {
        try
        {
            if (msg.StartsWith("Round finished! Anomalies: ") && type == ServerLogs.ServerLogType.GameEvent)
            {
                Synapse.Get<RoundEvents>().RoundEnd.Raise(new RoundEndEvent());
            }
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Round End Event Failed\n" + ex);
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.RpcRoundStarted))]
    public static void RoundStartPatch()
    {
        try
        {
            Synapse.Get<RoundEvents>().RoundStart.Raise(new RoundStartEvent());
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Round Start Event Failed\n" + ex);
        }
    }

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
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Round EndCheck Event Failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RoundRestart), nameof(RoundRestart.InitiateRoundRestart))]
    public static void RoundRestartPatch()
    {
        try
        {
            Synapse.Get<RoundEvents>().RoundRestart.Raise(new RoundRestartEvent());
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Events: Round Restart Event failed:\n" + ex);
        }
    }
}

public static class DecoratedRoundMethods
{
    // Decorated and refactored coroutine RoundSummary._ProcessServerSideCode()
    public static IEnumerator<float> ProcessServerSideCode(RoundSummary roundSummary)
    {
        // Neuron Event Hook
        var roundEvents = Synapse.Get<RoundEvents>();

        float time = Time.unscaledTime;
        while (roundSummary != null)
        {
            yield return Timing.WaitForSeconds(2.5f);
            if (RoundSummary.RoundLock || (roundSummary._keepRoundOnOne && PlayerManager.players.Count < 2) ||
                !RoundSummary.RoundInProgress() || !((double) Time.unscaledTime - (double) time >= 15.0)) continue;
            
            RoundSummary.SumInfo_ClassList classList = new RoundSummary.SumInfo_ClassList();
            foreach (KeyValuePair<GameObject, ReferenceHub> allHub in ReferenceHub.GetAllHubs())
            {
                if (!(allHub.Value == null))
                {
                    CharacterClassManager characterClassManager = allHub.Value.characterClassManager;
                    if (characterClassManager.Classes.CheckBounds(characterClassManager.CurClass))
                    {
                        switch (characterClassManager.CurRole.team)
                        {
                            case Team.SCP:
                                if (characterClassManager.CurClass == RoleType.Scp0492)
                                {
                                    ++classList.zombies;
                                    continue;
                                }

                                ++classList.scps_except_zombies;
                                continue;
                            case Team.MTF:
                                ++classList.mtf_and_guards;
                                continue;
                            case Team.CHI:
                                ++classList.chaos_insurgents;
                                continue;
                            case Team.RSC:
                                ++classList.scientists;
                                continue;
                            case Team.CDP:
                                ++classList.class_ds;
                                continue;
                            default:
                                continue;
                        }
                    }
                }
            }

            yield return float.NegativeInfinity;
            classList.warhead_kills =
                AlphaWarheadController.Host.detonated ? AlphaWarheadController.Host.warheadKills : -1;
            yield return float.NegativeInfinity;
            classList.time = (int) Time.realtimeSinceStartup;
            yield return float.NegativeInfinity;
            RoundSummary.roundTime = classList.time - roundSummary.classlistStart.time;
            int totalMtfs = classList.mtf_and_guards + classList.scientists;
            int totalChaos = classList.chaos_insurgents + classList.class_ds;
            int totalScps = classList.scps_except_zombies + classList.zombies;
            int totalClassD = classList.class_ds + RoundSummary.EscapedClassD;
            int totalScientists = classList.scientists + RoundSummary.EscapedScientists;
            RoundSummary.SurvivingSCPs = classList.scps_except_zombies;
            var shouldRoundEnd = false; // Change temporary variable instead of real field
            float percentageDClass = roundSummary.classlistStart.class_ds == 0
                ? 0.0f
                : (float) (totalClassD / roundSummary.classlistStart.class_ds);
            float percentageScientists = roundSummary.classlistStart.scientists == 0
                ? 1f
                : (float) (totalScientists / roundSummary.classlistStart.scientists);
            if (classList.class_ds <= 0 && totalMtfs <= 0)
            {
                shouldRoundEnd = true;
            }
            else
            {
                int livingTeams = 0;
                if (totalMtfs > 0)
                    ++livingTeams;
                if (totalChaos > 0)
                    ++livingTeams;
                if (totalScps > 0)
                    ++livingTeams;
                shouldRoundEnd = livingTeams <= 1;
            }
            
            int normalizedMtfs = totalMtfs > 0 ? 1 : 0;
            bool anyChaos = totalChaos > 0;
            bool anyScps = totalScps > 0;
            RoundSummary.LeadingTeam leadingTeam = RoundSummary.LeadingTeam.Draw;
            if (normalizedMtfs != 0)
                leadingTeam = RoundSummary.EscapedScientists >= RoundSummary.EscapedClassD
                    ? RoundSummary.LeadingTeam.FacilityForces
                    : RoundSummary.LeadingTeam.Draw;
            else if (anyScps || anyScps & anyChaos)
                leadingTeam = RoundSummary.EscapedClassD > RoundSummary.SurvivingSCPs
                    ? RoundSummary.LeadingTeam.ChaosInsurgency
                    : (RoundSummary.SurvivingSCPs > RoundSummary.EscapedScientists
                        ? RoundSummary.LeadingTeam.Anomalies
                        : RoundSummary.LeadingTeam.Draw);
            else if (anyChaos)
                leadingTeam = RoundSummary.EscapedClassD >= RoundSummary.EscapedScientists
                    ? RoundSummary.LeadingTeam.ChaosInsurgency
                    : RoundSummary.LeadingTeam.Draw;
            
            //
            NeuronLogger.For<Synapse>().Error($"Synapse Round End Check {shouldRoundEnd}, Leading {leadingTeam}");
            roundSummary.RoundEnded = shouldRoundEnd;
            
            if (!roundSummary.RoundEnded) continue; // Perform Round End
            FriendlyFireConfig.PauseDetector = true;
            string str = "Round finished! Anomalies: " + totalScps + " | Chaos: " + totalChaos +
                         " | Facility Forces: " + totalMtfs + " | D escaped percentage: " + percentageDClass +
                         " | S escaped percentage: : " + percentageScientists;
            GameCore.Console.AddLog(str, Color.gray);
            ServerLogs.AddLog(ServerLogs.Modules.Logger, str, ServerLogs.ServerLogType.GameEvent);
            yield return Timing.WaitForSeconds(1.5f);
            int roundCd = Mathf.Clamp(ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);
            if (roundSummary != null)
                roundSummary.RpcShowRoundSummary(roundSummary.classlistStart, classList, leadingTeam,
                    RoundSummary.EscapedClassD, RoundSummary.EscapedScientists, RoundSummary.KilledBySCPs,
                    roundCd);
            yield return Timing.WaitForSeconds((float) (roundCd - 1));
            roundSummary.RpcDimScreen();
            yield return Timing.WaitForSeconds(1f);
            RoundRestart.InitiateRoundRestart();
        }
    }
}