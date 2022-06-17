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
public class RoundPatches
{
    [HarmonyPatch(typeof(SpawnpointManager), nameof(SpawnpointManager.FillSpawnPoints))]
    [HarmonyPrefix]
    public static void RoundWaitingPatch()
    {
        NeuronLogger.For<RoundPatches>().Info("Waiting for players event!");
    }


    [HarmonyPatch(typeof(ServerLogs), nameof(ServerLogs.AddLog))]
    [HarmonyPrefix]
    public static void RoundEndPatch(ref ServerLogs.Modules module, ref string msg, ref ServerLogs.ServerLogType type, ref bool init)
    {
        if (msg.StartsWith("Round finished! Anomalies: ") && type == ServerLogs.ServerLogType.GameEvent)
        {
            NeuronLogger.For<RoundPatches>().Error("Round end neuron event");
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.RpcRoundStarted))]
    [HarmonyPrefix]
    public static void RoundStartPatch()
    {
        NeuronLogger.For<RoundPatches>().Error("Round start neuron event");
    }

}

[Patches]
[HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.RpcRoundStarted))]
internal static class RoundStartPatch
{
    
    [HarmonyPrefix]
    private static void RoundStart()
    {
        try
        {
            NeuronLogger.For<RoundPatches>().Info("Round start event!");
        }
        catch (Exception e)
        {
            NeuronLogger.For<RoundPatches>().Error($"Synapse-Event: RoundStartEvent failed!!\n{e}");
        }
    }
}


public class DecoratedRoundMethods
{
    [HarmonyPatch(typeof(RoundSummary), "Start", MethodType.Normal), HarmonyPrefix]
    public static bool RoundSummaryOverride(RoundSummary __instance, out IEnumerator<float> __result)
    {
        __result = DecoratedRoundMethods.ProcessServerSideCode(__instance);
        return false;
    }
    
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
            NeuronLogger.For<RoundPatches>().Error($"Synapse Round End Check {shouldRoundEnd}, Leading {leadingTeam}");
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