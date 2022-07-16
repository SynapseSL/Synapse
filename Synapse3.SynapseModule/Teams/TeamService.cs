using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Ninject;
using Respawning;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Teams;

public class TeamService : Service
{
    private readonly List<ISynapseTeam> _teams = new();
    private readonly IKernel _kernel;

    public TeamService(IKernel kernel)
    {
        _kernel = kernel;
    }

    public int NextTeam { get; internal set; } = -1;

    public ReadOnlyCollection<ISynapseTeam> Teams => _teams.AsReadOnly();

    internal void LoadBinding(SynapseTeamBinding binding) => RegisterTeam(binding.Type, binding.Info);
    
    public void RegisterTeam<TTeam>() where TTeam : ISynapseTeam
    {
        var type = typeof(TTeam);
        var info = type.GetCustomAttribute<TeamAttribute>();
        if (info == null) return;
        RegisterTeam(type, info);
    }

    public void RegisterTeam(Type teamType, TeamAttribute info)
    {
        if(IsIdRegistered(info.Id)) return;
        if(!typeof(ISynapseTeam).IsAssignableFrom(teamType)) return;
        
        var teamHandler = (ISynapseTeam)_kernel.Get(teamType);
        _kernel.Bind(teamType).ToConstant(teamHandler).InSingletonScope();
        teamHandler.Info = info;
        _teams.Add(teamHandler);
    }

    public ISynapseTeam GetTeam(int id) => _teams.FirstOrDefault(x => x.Info.Id == id);

    public bool IsIdRegistered(int id)
        => IsDefaultId(id) || _teams.Any(x => x.Info.Id == id);

    public bool IsDefaultId(int id)
        => id is >= (int)Team.SCP and <= (int)Team.TUT;
    
    public bool IsDefaultSpawnableID(int id) 
        => id is (int)Team.MTF or (int)Team.CHI;

    public float GetRespawnTime(int id)
    {
        switch (id)
        {
            case 0: return 0;
            case 1:
            case 2:
                if (RespawnWaveGenerator.SpawnableTeams.TryGetValue((SpawnableTeamType)id, out var handler))
                    return handler.EffectTime;
                return 0;

            default:
                var team = GetTeam(id);
                if (team == null) return 0;
                return team.RespawnTime;
        }
    }

    public void ExecuteRespawnAnnouncement(int id)
    {
        switch (id)
        {
            case 0: return;
            case 1:
            case 2:
                RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.Selection,
                    (SpawnableTeamType)id);
                break;
            
            default:
                var team = GetTeam(id);
                team?.RespawnAnnouncement();
                break;
        }
    }
    
    public void SpawnTeam(int id, List<SynapsePlayer> players)
    {
        if (IsDefaultSpawnableID(id))
        {
            Synapse.Get<RoundService>().SpawnWave(id == (int)Team.CHI);
            return;
        }

        var team = GetTeam(id);
        if(team == null) return;

        if (players.Count > team.MaxWaveSize)
            players = players.GetRange(0, team.MaxWaveSize);
        
        if(players.Count == 0) return;

        team.SpawnPlayers(players);
    }

    public void Spawn()
    {
        //TODO: Create own Spawn so that Custom Teams can actually spawn
        RespawnManager.Singleton.NextKnownTeam = (SpawnableTeamType)NextTeam;
        RespawnManager.Singleton.Spawn();
    }
}