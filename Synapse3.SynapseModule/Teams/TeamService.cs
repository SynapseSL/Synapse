using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Neuron.Core;
using Neuron.Core.Meta;
using Ninject;
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
        teamHandler.Info = info;
        _teams.Add(teamHandler);
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

    public ISynapseTeam GetTeam(int id) => _teams.FirstOrDefault(x => x.Info.Id == id);

    public bool IsIdRegistered(int id)
        => IsDefaultId(id) || _teams.Any(x => x.Info.Id == id);

    public bool IsDefaultId(int id)
        => id is >= (int)Team.SCP and <= (int)Team.TUT;
    
    public bool IsDefaultSpawnableID(int id) 
        => id is (int)Team.MTF or (int)Team.CHI;
}