using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Neuron.Core.Meta;
using Ninject;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Database;

public class DatabaseService : Service
{
    private readonly Synapse _synapseModule;
    public DatabaseService(Synapse synapseModule) => _synapseModule = synapseModule;

    public override void Enable()
    {
        while (_synapseModule.ModuleDataBaseBindingQueue.Count != 0)
        {
            var binding = _synapseModule.ModuleDataBaseBindingQueue.Dequeue();
            LoadBinding(binding);
        }
    }

    private List<IDatabase> _dataBases = new();
    public ReadOnlyCollection<IDatabase> DataBases => _dataBases.AsReadOnly();

    public bool IsIdRegistered(uint id) => _dataBases.Any(x => x.Attribute.Id == id);

    public void RegisterDataBase<TDatabase>() where TDatabase : IDatabase => RegisterDataBase(typeof(TDatabase));

    public void RegisterDataBase(Type dataBaseType)
    {
        var info = dataBaseType.GetCustomAttribute<DatabaseAttribute>();
        if (info == null) return;
        RegisterDataBase(info);
    }

    public void RegisterDataBase(DatabaseAttribute info)
    {
        if (info.DataBaseType == null) return;
        if (IsIdRegistered(info.Id)) return;

        var dataBase = (IDatabase)Synapse.GetOrCreate(info.DataBaseType);
        dataBase.Attribute = info;
        dataBase.Load();
        
        _dataBases.Add(dataBase);
        _dataBases = _dataBases.OrderByDescending(x => x.Attribute.Priority).ToList();
    }

    public string GetPlayerData(SynapsePlayer player, string key)
    {
        foreach (var dataBase in DataBases)
        {
            var info = dataBase.GetPlayerData(player, key, out var handled);
            if (handled)
                return info;
        }

        return "";
    }

    public void SetPlayerData(SynapsePlayer player, string key, string value)
    {
        foreach (var dataBase in DataBases)
        {
            dataBase.SetPlayerData(player, key, value, out var handled);
            if (handled) return;
        }
    }
    
    public void DeletePlayerData(SynapsePlayer player, string key)
    {
        foreach (var dataBase in DataBases)
        {
            dataBase.DeletePlayerData(player, key, out var handled);
            if (handled) return;
        }
    }

    public string GetData(string key)
    {
        foreach (var dataBase in DataBases)
        {
            var info = dataBase.GetData(key, out var handled);
            if (handled)
                return info;
        }

        return "";
    }
    
    public void SetData(string key, string value)
    {
        foreach (var dataBase in DataBases)
        {
            dataBase.SetData(key, value, out var handled);
            if (handled) return;
        }
    }
    
    public void DeleteData(string key, string value)
    {
        foreach (var dataBase in DataBases)
        {
            dataBase.DeleteData(key, out var handled);
            if (handled) return;
        }
    }

    public Dictionary<string, string> GetLeaderBoard(string key, bool orderFromHighest = true, ushort size = 0)
    {
        foreach (var dataBase in DataBases)
        {
            var leaderBoard = dataBase.GetLeaderBoard(key, out var handled, orderFromHighest, size);
            if (handled) return leaderBoard;
        }

        return new Dictionary<string, string>();
    }

    internal void LoadBinding(SynapseDataBaseBinding binding) => RegisterDataBase(binding.Info);
}