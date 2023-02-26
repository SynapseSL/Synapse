using GameCore;
using HarmonyLib;
using MEC;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using PluginAPI.Enums;
using PluginAPI.Events;
using Respawning;
using RoundRestarting;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;
using Synapse3.SynapseModule.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Respawning.RespawnManager;
using Console = GameCore.Console;

namespace Synapse3.SynapseModule.Patching.Patches;

#if !PATCHLESS
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
            SynapseLogger<Synapse>.Error("Round EndCheckPatch Failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("FirstSpawn", PatchType.RoundEvent)]
public static class FirstSpawnPatch
{
    internal static List<SynapsePlayer> _blockedPlayer = new();
    private static readonly RoundEvents _round;
    static FirstSpawnPatch() => _round = Synapse.Get<RoundEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RoleAssigner), nameof(RoleAssigner.OnRoundStarted))]
    public static bool OnRoundStarted()
    {
        try
        {
            var queueString = ConfigFile.ServerConfig.GetString("team_respawn_queue", "4014314031441404134041434414");
            var length = queueString.Length;
            if (RoleAssigner._prevQueueSize < length)
            {
                RoleAssigner._totalQueue = new Team[length];
                RoleAssigner._humanQueue = new Team[length];
                RoleAssigner._prevQueueSize = length;
            }

            var humanQueueLength = 0;
            var totalQueueLength = 0;

            foreach (var teamLetter in queueString)
            {
                var team = (Team)(teamLetter - '0');
                if (!Enum.IsDefined(typeof(Team), team)) continue;
                if (team != Team.SCPs)
                    RoleAssigner._humanQueue[humanQueueLength++] = team;
                RoleAssigner._totalQueue[totalQueueLength++] = team;
            }

            if (totalQueueLength == 0) return false;

            var amountOfPlayers = ReferenceHub.AllHubs.Count(RoleAssigner.CheckPlayer);

            var amountOfScp = 0;
            for (var i = 0; i < amountOfPlayers; i++)
            {
                if (RoleAssigner._totalQueue[i % totalQueueLength] == Team.SCPs)
                    amountOfScp++;
            }

            //TODO: Check why the Player is kicked when EnableNormalSpawning is false during the late join period
            var ev = new FirstSpawnEvent()
            {
                AmountOfScpSpawns = amountOfScp,
                HumanQueue = RoleAssigner._humanQueue
            };
            _round.FirstSpawn.RaiseSafely(ev);
            _blockedPlayer = ev.PlayersBlockedFromSpawning;
            if (ev.EnableLateJoin)
            {
                RoleAssigner._spawned = true;
                RoleAssigner.LateJoinTimer.Restart();
            }

            if (!ev.EnableNormalSpawning) return false;

            ScpSpawner.SpawnScps(ev.AmountOfScpSpawns);
            HumanSpawner.SpawnHumans(ev.HumanQueue, ev.HumanQueue.Length);
            foreach (var hub in ReferenceHub.AllHubs.Where(hub => hub.IsAlive()))
            {
                RoleAssigner.AlreadySpawnedPlayers.Add(hub.characterClassManager.UserId);
            }
            _blockedPlayer.Clear();
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Role Assign Patch failed\n" + ex);
            return true;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RoleAssigner), nameof(RoleAssigner.CheckPlayer))]
    public static void CheckPlayer(ref bool __result, ReferenceHub hub)
    {
        if (_blockedPlayer.Any(x => x.Hub == hub))
            __result = false;
    }
}

[Automatic]
[SynapsePatch("TeamSelect", PatchType.RoundEvent)]
public static class TeamSelectPatch
{
    private static readonly TeamService _team;
    private static readonly RoundEvents _round;
    static TeamSelectPatch()
    {
        _team = Synapse.Get<TeamService>();
        _round = Synapse.Get<RoundEvents>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RespawnManager), nameof(RespawnManager.Update))]
    public static bool OnTeamUpdate(RespawnManager __instance)
    {
        try
        {
            if (!__instance.ReadyToCommence()) return false;
            if (__instance._stopwatch.Elapsed.TotalSeconds > __instance._timeForNextSequence)
                __instance._curSequence++;

            switch (__instance._curSequence)
            {
                case RespawnSequencePhase.SelectingTeam:
                    if (!ReferenceHub.AllHubs.Any(__instance.CheckSpawnable))
                    {
                        __instance.RestartSequence();
                        return false;
                    }
                    var dominatingTeam = RespawnTokensManager.DominatingTeam;

                    var ev = new SelectTeamEvent
                    {
                        TeamId = (uint)dominatingTeam,
                        Reset = !EventManager.ExecuteEvent(ServerEventType.TeamRespawnSelected, dominatingTeam),
                    };
                    _round.SelectTeam.RaiseSafely(ev);
                    var nextTeam = ev.TeamId;

                    dominatingTeam = _team.IsDefaultSpawnableID(nextTeam) ?
                        (SpawnableTeamType)nextTeam : SpawnableTeamType.None;

                    if (ev.Reset)
                    {
                        __instance.RestartSequence();
                        return false;
                    }
                    _team.NextTeam = ev.TeamId;

                    __instance.NextKnownTeam = dominatingTeam;
                    __instance._curSequence = RespawnSequencePhase.PlayingEntryAnimations;
                    __instance._stopwatch.Restart();
                    __instance._timeForNextSequence = _team.GetRespawnTime(ev.TeamId);
                    _team.ExecuteRespawnAnnouncement(nextTeam);
                    break;

                case RespawnSequencePhase.SpawningSelectedTeam:
                    _team.Spawn();
                    __instance.RestartSequence();
                    break;
            }
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: SelectTeam Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RespawnManager), nameof(RespawnManager.ForceSpawnTeam))]
    public static bool ForceRespawn(RespawnManager __instance, SpawnableTeamType teamToSpawn)
    {
        try
        {
            _team.NextTeam = (uint)teamToSpawn;
            __instance.NextKnownTeam = teamToSpawn;
            _team.Spawn();
            __instance.RestartSequence();
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Team: Force Team Respawn failed\n" + ex);
            return false;
        }
    }
}

public static class DecoratedRoundMethods
{
    private static readonly PlayerService PlayerService;
    private static readonly RoundService RoundService;
    private static readonly RoundEvents RoundEvents;
    private static readonly SynapseConfigService ConfigService;

    static DecoratedRoundMethods()
    {
        PlayerService = Synapse.Get<PlayerService>();
        RoundService = Synapse.Get<RoundService>();
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
            if (!RoundSummary.RoundInProgress() || Time.unscaledTime - time < 15f) continue;

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
            if (!ConfigService.GamePlayConfiguration.ChaosAndScpEnemy && roundData.class_ds <= 0 &&
                numberOfFoundationStaff <= 0)
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
                    if (!customRole.GetEnemiesID().Any(x => livingTeams.Contains(x)))
                        continue;

                    summary._roundEnded = false;
                    break;
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
                    ? RoundSummary.LeadingTeam.ChaosInsurgency
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
            RoundEvents.CheckEnd.RaiseSafely(ev);
            summary._roundEnded = ev.EndRound;

            if (!RoundService.ForceEnd && !summary._roundEnded) continue;

            RoundEvents.End.RaiseSafely(new RoundEndEvent(leadingTeam));

            if (!EventManager.ExecuteEvent(ServerEventType.RoundEnd, leadingTeam)) continue;
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
#endif
