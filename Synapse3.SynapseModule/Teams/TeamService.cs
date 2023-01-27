using Neuron.Core.Meta;
using Ninject;
using PlayerRoles;
using PluginAPI.Enums;
using Respawning;
using Respawning.NamingRules;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Synapse3.SynapseModule.Teams;

public class TeamService : Service
{
    private readonly List<ISynapseTeam> _teams = new();
    private readonly IKernel _kernel;
    private readonly Synapse _synapseModule;

    public TeamService(IKernel kernel, Synapse synapseModule)
    {
        _kernel = kernel;
        _synapseModule = synapseModule;
    }

    public override void Enable()
    {
        while (_synapseModule.ModuleTeamBindingQueue.Count != 0)
        {
            var binding = _synapseModule.ModuleTeamBindingQueue.Dequeue();
            LoadBinding(binding);
        }
    }

    public uint NextTeam { get; internal set; } = uint.MaxValue;

    public ReadOnlyCollection<ISynapseTeam> Teams => _teams.AsReadOnly();

    internal void LoadBinding(SynapseTeamBinding binding) => RegisterTeam(binding.Type, binding.Info);

    /// <summary>
    /// Create a instance of the given type and register it as Team in Synapse.It also binds the created Instance to the kernel
    /// </summary>
    public void RegisterTeam(Type teamType, TeamAttribute info)
    {
        if(IsIdRegistered(info.Id)) return;
        if(!typeof(ISynapseTeam).IsAssignableFrom(teamType)) return;
        
        var teamHandler = (ISynapseTeam)_kernel.Get(teamType);
        _kernel.Bind(teamType).ToConstant(teamHandler).InSingletonScope();
        
        teamHandler.Attribute = info;
        teamHandler.Load();
        
        _teams.Add(teamHandler);
    }

    /// <summary>
    /// Register the given Team without binding it to the kernel
    /// </summary>
    public void RegisterTeam(ISynapseTeam team, TeamAttribute info)
    {
        if(IsIdRegistered(info.Id)) return;
        team.Attribute = info;
        _teams.Add(team);
    }

    public ISynapseTeam GetTeam(uint id) => _teams.FirstOrDefault(x => x.Attribute.Id == id);

    public string GetTeamName(uint id)
    {
        return id switch
        {
            0 => "SCP",
            1 => "Foundation Forces",
            2 => "Chaos Insurgency",
            3 => "Scientist",
            4 => "Class-D",
            5 => "Spectator",
            6 => "Tutorial",
            _ => GetTeam(id)?.Attribute.Name ?? ""
        };
    }

    public bool IsIdRegistered(uint id)
        => IsDefaultId(id) || _teams.Any(x => x.Attribute.Id == id);

    public bool IsDefaultId(uint id)
        => id is >= (uint)Team.SCPs and <= (uint)Team.OtherAlive;
    
    public bool IsDefaultSpawnableID(uint id) 
        => id is (uint)Team.FoundationForces or (uint)Team.ChaosInsurgency;

    public float GetRespawnTime(uint id)
    {
        switch (id)
        {
            case 0: return 0;
            case 1:
            case 2: 
                if (RespawnManager.SpawnableTeams.TryGetValue((SpawnableTeamType)id, out var handler))
                    return handler.EffectTime;
                return 0;

            default:
                var team = GetTeam(id);
                if (team == null) return 0;
                return team.RespawnTime;
        }
    }

    public int GetMaxWaveSize(uint id, bool addTickets = false)
    {
        switch (id)
        {
            case 0: return 0;
            
            case 1:
            case 2:
                return RespawnManager.SpawnableTeams.TryGetValue((SpawnableTeamType)id, out var handler) ? handler.MaxWaveSize : 0;

            default:
                if (!IsIdRegistered(id)) return 0;

                var team = GetTeam(id);
                return team.MaxWaveSize;
        }
    }

    public void ExecuteRespawnAnnouncement(uint id)
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
    
    public void SpawnCustomTeam(uint id, List<SynapsePlayer> players)
    {
        if (IsDefaultSpawnableID(id)) return;

        var team = GetTeam(id);
        if (team == null) return;

        if (players.Count > team.MaxWaveSize)
            players = players.GetRange(0, team.MaxWaveSize);
        
        if(players.Count == 0) return;

        team.SpawnPlayers(players);
    }

    public void Spawn()
    {
        if (NextTeam == uint.MaxValue)
            goto ResetTeam;

        var players = Synapse.Get<PlayerService>().Players.ToList();
        players = players.Where(x => x.RoleID == (uint)RoleTypeId.Spectator).ToList();

        if (Synapse.Get<RoundService>().PrioritySpawn)
        {
            players = players.OrderBy(x => x.DeathTime).ToList();
        }
        else
        {
            players.ShuffleList();
        }

        while (players.Count > GetMaxWaveSize(NextTeam))
        {
            players.RemoveAt(players.Count - 1);
        }

        var ev = new SpawnTeamEvent(NextTeam)
        {
            Players = players
        };
        Synapse.Get<RoundEvents>().SpawnTeam.Raise(ev);
        players = ev.Players;

        if (!ev.Allow || players.Count == 0)
            goto ResetTeam;

        while (players.Count > GetMaxWaveSize(NextTeam))
        {
            players.RemoveAt(players.Count - 1);
        }

        players.ShuffleList();

        //var unitName = _unit.PrepareSpawnNewUnit(_unit.GetUnitIdFromTeamId(NextTeam), players);

        switch (NextTeam)
        {
            case 1:
            case 2:
                if (!RespawnManager.SpawnableTeams.TryGetValue((SpawnableTeamType)NextTeam, out var handlerBase))
                    goto ResetTeam;

                RespawnTokensManager.ResetTokens();

                if (!PluginAPI.Events.EventManager.ExecuteEvent(ServerEventType.TeamRespawn, (SpawnableTeamType)NextTeam))
                {
                    RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.UponRespawn, (SpawnableTeamType)NextTeam);
                    return;
                }

                var roles = new Queue<RoleTypeId>();
                handlerBase.GenerateQueue(roles, players.Count);

                if (UnitNamingRule.TryGetNamingRule((SpawnableTeamType)NextTeam, out var rule))
                {
                    UnitNameMessageHandler.SendNew((SpawnableTeamType)NextTeam, rule);
                }

                foreach (var player in players)
                {
                    var role = roles.Dequeue();
                    player.RemoveCustomRole(DeSpawnReason.API);
                    player.RoleManager.ServerSetRole(role, RoleChangeReason.Respawn);
                }

                RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.UponRespawn,
                    (SpawnableTeamType)NextTeam);
                break;

            default:
                var team = GetTeam(NextTeam);
                if (team == null)
                    goto ResetTeam;

                team.SpawnPlayers(players);
                break;
        }

    ResetTeam:
        NextTeam = uint.MaxValue;
    }
}