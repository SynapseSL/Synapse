using System;
using System.Collections.Generic;
using System.Linq;
using GameCore;
using HarmonyLib;
using LightContainmentZoneDecontamination;
using MapGeneration.Distributors;
using MEC;
using Neuron.Core.Logging;
using RoundRestarting;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;
using UnityEngine;
using Console = GameCore.Console;

// ReSharper disable InconsistentNaming

namespace Synapse3.SynapseModule.Patches;

[Patches]
[HarmonyPatch]
internal static class RoundPatches
{
    private static bool _firstTime = true;
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StructureDistributor), nameof(StructureDistributor.PlaceSpawnables))]
    public static void RoundWaitingPatch()
    {
        try
        {
            var ev = new RoundWaitingEvent
            {
                FirstTime = _firstTime
            };
            Synapse.Get<RoundEvents>().Waiting.Raise(ev);
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
                Synapse.Get<RoundEvents>().End.Raise(new RoundEndEvent());
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
            Synapse.Get<RoundEvents>().Start.Raise(new RoundStartEvent());
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
            Synapse.Get<RoundEvents>().Restart.Raise(new RoundRestartEvent());
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Events: Round Restart Event failed:\n" + ex);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetRandomRoles))]
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DecontaminationController), nameof(DecontaminationController.FinishDecontamination))]
    public static bool OnDecontamination()
    {
        try
        {
            var ev = new DecontaminationEvent();
            Synapse.Get<RoundEvents>().Decontamination.Raise(ev);
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Events: Decontamination Event failed:\n" + ex);
            return true;
        }
    }
}

internal static class DecoratedRoundMethods
{
    // Decorated and refactored coroutine RoundSummary._ProcessServerSideCode()
    public static IEnumerator<float> ProcessServerSideCode(RoundSummary roundSummary)
    {
        // Neuron Event Hook
        var roundEvents = Synapse.Get<RoundEvents>();
        var playerService = Synapse.Get<PlayerService>();
        var roundService = Synapse.Get<RoundService>();

        float time = Time.unscaledTime;
        while (roundSummary != null)
        {
            yield return Timing.WaitForSeconds(2.5f);

            if (!RoundSummary.RoundInProgress()) continue;
            
            if(!roundService.ForceEnd)
                if (RoundSummary.RoundLock || (roundSummary._keepRoundOnOne && PlayerManager.players.Count < 2) ||
                    Time.unscaledTime - time < 15f)
                    continue;
            
            var classList = new RoundSummary.SumInfo_ClassList();
            var customRoles = new List<ISynapseRole>();
            var livingTeamsById = new List<uint>();

            foreach (var player in playerService.Players)
            {
                if(!player.ClassManager.Classes.CheckBounds(player.ClassManager.CurClass)) continue;

                livingTeamsById.Add(player.TeamID);
                if(player.CustomRole != null)
                    customRoles.Add(player.CustomRole);
                
                switch (player.TeamID)
                {
                    case (int)Team.SCP when player.RoleType == RoleType.Scp0492:
                        classList.zombies++;
                        break;
                    
                    case (int)Team.SCP when player.RoleType != RoleType.Scp0492:
                        classList.scps_except_zombies++;
                        break;
                    
                    case (int)Team.MTF:
                        classList.mtf_and_guards++;
                        break;
                    
                    case (int)Team.CHI:
                        classList.chaos_insurgents++;
                        break;
                    
                    case (int)Team.RSC:
                        classList.scientists++;
                        break;
                        
                    case (int)Team.CDP:
                        classList.class_ds++;
                        break;
                }
            }

            yield return Timing.WaitForOneFrame;
            
            classList.warhead_kills =
                AlphaWarheadController.Host.detonated ? AlphaWarheadController.Host.warheadKills : -1;
            
            yield return Timing.WaitForOneFrame;
            
            classList.time = (int) Time.realtimeSinceStartup;
            
            yield return Timing.WaitForOneFrame;
            
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
                : totalClassD / roundSummary.classlistStart.class_ds;
            float percentageScientists = roundSummary.classlistStart.scientists == 0
                ? 1f
                : totalScientists / roundSummary.classlistStart.scientists;
            
            if (classList.class_ds <= 0 && totalMtfs <= 0)
            {
                shouldRoundEnd = true;
            }
            else
            {
                int livingTeams = 0;
                
                if (totalMtfs > 0)
                    livingTeams++;
                
                if (totalChaos > 0)
                    livingTeams++;
                
                if (totalScps > 0)
                    livingTeams++;
                
                shouldRoundEnd = livingTeams <= 1;
            }

            var normalizedMtfs = totalMtfs > 0;
            var anyChaos = totalChaos > 0;
            var anyScps = totalScps > 0;
            
            var leadingTeam = RoundSummary.LeadingTeam.Draw;
            
            if (normalizedMtfs)
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

            if (shouldRoundEnd)
            {
                foreach (var role in customRoles)
                {
                    if (role.GetEnemiesID().Any(x => livingTeamsById.Contains(x)))
                    {
                        shouldRoundEnd = false;
                        break;
                    }
                }
            }
            
            var ev = new RoundCheckEndEvent
            {
                EndRound = shouldRoundEnd,
                WinningTeam = leadingTeam
            };

            try
            {
                roundEvents.CheckEnd.Raise(ev);
            }
            catch (Exception ex)
            {
                NeuronLogger.For<Synapse>().Error("Sy3 Events: Round Restart Event IEnumerator failed:\n" + ex);
            }

            leadingTeam = ev.WinningTeam;
            roundSummary.RoundEnded = ev.EndRound;
            
            if (!roundSummary.RoundEnded) continue; // Perform Round End
            
            FriendlyFireConfig.PauseDetector = true;
            string str = "Round finished! Anomalies: " + totalScps + " | Chaos: " + totalChaos +
                         " | Facility Forces: " + totalMtfs + " | D escaped percentage: " + percentageDClass +
                         " | S escaped percentage: : " + percentageScientists;
            Console.AddLog(str, Color.gray);
            ServerLogs.AddLog(ServerLogs.Modules.Logger, str, ServerLogs.ServerLogType.GameEvent);
            
            yield return Timing.WaitForSeconds(1.5f);
            
            int roundCd = Mathf.Clamp(ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);
            
            if (roundSummary != null)
                roundSummary.RpcShowRoundSummary(roundSummary.classlistStart, classList, leadingTeam,
                    RoundSummary.EscapedClassD, RoundSummary.EscapedScientists, RoundSummary.KilledBySCPs,
                    roundCd);
            
            yield return Timing.WaitForSeconds(roundCd - 1);
            
            roundSummary.RpcDimScreen();
            
            yield return Timing.WaitForSeconds(1f);
            
            RoundRestart.InitiateRoundRestart();
        }
    }

    public static void OnSetRandomRoles(CharacterClassManager manager, bool first)
    {
        if(!manager.isLocalPlayer || !manager.isServer) return;
        manager.RunDefaultClassPicker(first, out var roles, out var playersRoleList);
        if (first && ConfigFile.ServerConfig.GetBool("smart_class_picker", true) && Console.EnableSCP)
        {
            manager.RunSmartClassPicker(roles, out playersRoleList);
        }

        var players = new Dictionary<SynapsePlayer, uint>();
        foreach (var pair in playersRoleList)
        {
            var player = pair.Key.GetSynapsePlayer();
            if (player == null || player.OverWatch) continue;
            players[player] = (uint)pair.Value;
        }

        var ev = new FirstSpawnEvent
        {
            PlayerAndRoles = players
        };
        Synapse.Get<RoundEvents>().FirstSpawn.Raise(ev);
        
        players = ev.PlayerAndRoles;
        
        foreach (var pair in players)
        {
            //That way we can also apply the RoundStart SpawnReason otherwise we could just use the else part
            if (pair.Value is >= 0 and <= RoleService.HighestRole)
            {
                var chaos = manager.IsCIOnStart && pair.Value == (int)RoleType.FacilityGuard;

                manager.SetPlayersClass(chaos ? RoleType.ChaosRifleman : (RoleType)pair.Value, pair.Key.gameObject,
                    CharacterClassManager.SpawnReason.RoundStart);
            }
            else
            {
                pair.Key.RoleID = pair.Value;
            }
        }
    }
}