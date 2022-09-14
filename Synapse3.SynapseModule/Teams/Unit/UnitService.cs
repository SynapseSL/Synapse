using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Mirror;
using Neuron.Core.Meta;
using Ninject;
using Respawning;
using Respawning.NamingRules;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Teams.Unit;

public class UnitService : Service
{
    private readonly IKernel _kernel;
    private readonly Dictionary<SynapsePlayer, UnitInfo> _storedUnits = new ();
    private readonly List<ISynapseUnit> _units = new();
    private Dictionary<uint, byte> _teamToUnit = new()
    {
        //This just assign MTF to the default Mtf unit
        { 2, 2 }
    };

    public UnitService(IKernel kernel)
    {
        _kernel = kernel;
    }
    
    public SyncList<SyncUnit> UnitList => RespawnManager.Singleton.NamingManager.AllUnitNames;

    public override void Enable()
    {
        RegisterUnit<MtfUnit>();
    }

    public ReadOnlyCollection<ISynapseUnit> Units => _units.AsReadOnly();

    public void RegisterUnit<TUnit>()
    {
        var info = (UnitAttribute)typeof(TUnit).GetCustomAttribute(typeof(UnitAttribute));
        if(info == null) return;
        RegisterUnit(typeof(TUnit), info);
    }

    public void RegisterUnit(Type type)
    {
        var info = (UnitAttribute)type.GetCustomAttribute(typeof(UnitAttribute));
        if(info == null) return;
        RegisterUnit(type, info);
    }

    public void RegisterUnit<TUnit>(UnitAttribute info) => RegisterUnit(typeof(TUnit), info);

    public void RegisterUnit(Type type, UnitAttribute info)
    {
        if (IsIdRegistered(info.Id)) return;
        if (!typeof(ISynapseUnit).IsAssignableFrom(type)) return;
        
        var synapseUnit = (ISynapseUnit)_kernel.Get(type);
        _kernel.Bind(type).ToConstant(synapseUnit).InSingletonScope();
        
        synapseUnit.Attribute = info;
        synapseUnit.Load();
        
        _units.Add(synapseUnit);
        if (info.AssignedTeam != 0)
            _teamToUnit[info.AssignedTeam] = info.Id;
    }

    public bool IsIdRegistered(byte id) => _units.Any(x => x.Attribute.Id == id);

    public string PrepareSpawnNewUnit(byte unitId, List<SynapsePlayer> players)
    {
        if (unitId == 0) return null;
        
        var unit = _units.FirstOrDefault(x => x.Attribute.Id == unitId);
        if (unit == null) return null;
        var newUnit = unit.GetNewUnit();
        
        foreach (var player in players)
        {
            _storedUnits[player] = new UnitInfo()
            {
                UnitName = newUnit,
                UnitId = unitId
            };
        }

        return newUnit;
    }

    public void AddUnit(string name, byte id, int index = int.MaxValue)
    {
        var firstEntry = -1;
        var lastEntry = 0;

        for (int i = 0; i < UnitList.Count; i++)
        {
            var syncUnit = UnitList[i];
            if (syncUnit.SpawnableTeam != id) continue;

            if (firstEntry == -1)
                firstEntry = i;
            
            lastEntry = i;
        }

        if (firstEntry == -1)
            firstEntry = 0;

        var maxIndex = lastEntry - firstEntry + 1;
        if (index > maxIndex)
            index = maxIndex;

        var newSyncUnit = new SyncUnit()
        {
            SpawnableTeam = id,
            UnitName = name
        };

        if (index < UnitList.Count)
            UnitList.Insert(firstEntry + index, newSyncUnit);
        else 
            UnitList.Add(newSyncUnit);
    }

    public UnitInfo GetPlayerUnit(SynapsePlayer player, uint roleId)
    {
        if (!_storedUnits.ContainsKey(player)) return GetUnitForRole(roleId);
        
        var unit = _storedUnits[player];
        _storedUnits.Remove(player);
        return unit;
    }

    public UnitInfo GetUnitForRole(uint roleId)
    {
        var unit = _units.FirstOrDefault(x => x.Attribute.DefaultRolesInUnit.Contains(roleId));
        if (unit != null)
            return new UnitInfo()
            {
                UnitId = unit.Attribute.Id,
                UnitName = unit.DefaultUnit
            };

        return new UnitInfo()
        {
            UnitId = 0,
            UnitName = ""
        };
    }

    public byte GetUnitIdFromTeamId(uint teamId)
        => _teamToUnit.ContainsKey(teamId) ? _teamToUnit[teamId] : (byte)0;
}