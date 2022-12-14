using System;
using GameCore;
using Neuron.Core.Meta;
using Respawning;
using RoundRestarting;
using Synapse3.SynapseModule.Events;
using UnityEngine;

namespace Synapse3.SynapseModule;

public class RoundService : Service
{
    private RoundSummary Rs => RoundSummary.singleton;
    private RespawnManager Rm => RespawnManager.Singleton;
    private RoundEvents _round;

    public RoundService(RoundEvents round)
    {
        _round = round;
    }

    public override void Enable()
    {
       _round.Waiting.Subscribe(AddRound);
    }

    public override void Disable()
    {
        _round.Waiting.Unsubscribe(AddRound);
    }

    private void AddRound(RoundWaitingEvent ev)
    {
        CurrentRound++;
        ForceEnd = false;
        FemurSacrifices = 0;
    }


    /// <summary>
    /// The number of the round sciece the last server restart
    /// </summary>
    public int CurrentRound { get; private set; }

    public int FemurSacrifices { get; set; }

    /// <summary>
    /// The time until the next wave spawns
    /// </summary>
    public float NextSpawn
    {
        get => Rm._timeForNextSequence - (Rm._stopwatch.Elapsed.Hours * 3600 + Rm._stopwatch.Elapsed.Minutes * 60 + Rm._stopwatch.Elapsed.Seconds);
        set => Rm._timeForNextSequence = value + (Rm._stopwatch.Elapsed.Hours * 3600 + Rm._stopwatch.Elapsed.Minutes * 60 + Rm._stopwatch.Elapsed.Seconds);
    }

    /// <summary>
    /// If enabled the players that are the longest time dead will be prioritised when selecting the players to respawn
    /// </summary>
    public bool PrioritySpawn
    {
        get => Rm._prioritySpawn;
        set => Rm._prioritySpawn = value;
    }

    /// <summary>
    /// Prevents the server from starting the round
    /// </summary>
    public bool LobbyLock
    {
        get => RoundStart.LobbyLock;
        set => RoundStart.LobbyLock = value;
    }

    /// <summary>
    /// Prevents the server from ending the round
    /// </summary>
    public bool RoundLock
    {
        get => RoundSummary.RoundLock;
        set => RoundSummary.RoundLock = value;
    }

    /// <summary>
    /// The number of D-Personnel who escaped the facility (usable at the end of the round)
    /// </summary>
    public int EscapedDPersonnel
    {
        get => RoundSummary.EscapedClassD;
        set => RoundSummary.EscapedClassD = value;
    }
    
    /// <summary>
    /// The number of Scientists who escaped the facility (usable at the end of the round)
    /// </summary>
    public int EscapedScientists
    {
        get => RoundSummary.EscapedScientists;
        set => RoundSummary.EscapedScientists = value;
    }
    
    /// <summary>
    /// The number of kills by scps (usable at the end of the round)
    /// </summary>
    public int ScpKills
    {
        get => RoundSummary.KilledBySCPs;
        set => RoundSummary.KilledBySCPs = value;
    }

    /// <summary>
    /// The number of scps who survived the round (usable at the end of the round)
    /// </summary>
    public int SurvivingScps
    {
        get => RoundSummary.SurvivingSCPs;
        set => RoundSummary.SurvivingSCPs = value;
    }

    /// <summary>
    /// The number of players which got turned into SCP049-2 (usable at the end of the round)
    /// </summary>
    public int ChangedIntoZombies
    {
        get => RoundSummary.ChangedIntoZombies;
        set => RoundSummary.ChangedIntoZombies = value;
    }
    
    
    //TODO:
    /*
    /// <summary>
    /// The number of respawn tickets for mtf
    /// </summary>
    public int MtfTickets
    {
        get => RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox);
        set => RespawnTickets.Singleton._tickets[SpawnableTeamType.NineTailedFox] = value;
    }
    
    /// <summary>
    /// The number of respawn tickets for chaosinsugency
    /// </summary>
    public int ChaosTickets
    {
        get => RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.ChaosInsurgency);
        set => RespawnTickets.Singleton._tickets[SpawnableTeamType.ChaosInsurgency] = value;
    }
    */

    /// <summary>
    /// The length of the round
    /// </summary>
    public TimeSpan RoundLength => RoundStart.RoundLength;

    /// <summary>
    /// True during active round
    /// </summary>
    public bool RoundIsActive => RoundSummary.RoundInProgress();

    /// <summary>
    /// True when the round has ended
    /// </summary>
    public bool RoundEnded => Rs._roundEnded;
    
    internal bool ForceEnd { get; set; }

    /// <summary>
    /// Manual ForceRoundStart
    /// </summary>
    public void StartRound() => CharacterClassManager.ForceRoundStart();

    /// <summary>
    /// Manual ForceRoundEnd
    /// </summary>
    public void EndRound() => ForceEnd = true;

    /// <summary>
    /// Manual ForceRoundRestart
    /// </summary>
    public void RestartRound() => RoundRestart.InitiateRoundRestart();
    
    /// <summary>
    /// Spawn the wave vehicle
    /// </summary>
    /// <param name="isCI"></param>
    public void SpawnVehicle(SpawnableTeamType team)
    {
        if (team == SpawnableTeamType.None) return;
        RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.Selection, team);
    } 

    /// <summary>
    /// Play the sound d-personnel hears, when ci respawns
    /// </summary>
    public void PlayChaosSpawnSound() => RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.UponRespawn, SpawnableTeamType.ChaosInsurgency);

    /// <summary>
    /// Dim the players screens to black
    /// </summary>
    public void DimScreens() => Rs.RpcDimScreen();

    /// <summary>
    /// Manual ForceShow the RoundSummary
    /// </summary>
    /// <param name="remainingPlayers"></param>
    /// <param name="leadingTeam"></param>
    public void ShowRoundSummary(RoundSummary.SumInfo_ClassList remainingPlayers, RoundSummary.LeadingTeam leadingTeam)
    {
        var timeToRoundRestart = Mathf.Clamp(ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);
        
        //TODO:
        //Rs.RpcShowRoundSummary(Rs.classlistStart, remainingPlayers, leadingTeam, EscapedDPersonnel, EscapedScientists, ScpKills, timeToRoundRestart);
    }

    /// <summary>
    /// Manually force a wave to respawn
    /// </summary>
    /// <param name="isCI"></param>
    public void SpawnWave(SpawnableTeamType team)
    {
        if (team == SpawnableTeamType.None) return;
        RespawnManager.Singleton.ForceSpawnTeam(team);
    }
}