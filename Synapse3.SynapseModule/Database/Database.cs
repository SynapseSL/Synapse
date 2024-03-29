﻿using System.Collections.Generic;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Database;

public abstract class Database : InjectedLoggerBase, IDatabase
{
    public virtual string GetPlayerData(SynapsePlayer player, string key, out bool isHandled)
    {
        isHandled = false;
        return "";
    }

    public virtual void SetPlayerData(SynapsePlayer player, string key, string value, out bool isHandled) => isHandled = false;
    public virtual void DeletePlayerData(SynapsePlayer player, string key, out bool isHandled) => isHandled = false;
    public virtual bool IsPlayerKeySet(SynapsePlayer player, string key) => false;

    public virtual Dictionary<string, string> GetAllPlayerData(SynapsePlayer player, out bool isHandled)
    {
        isHandled = false;
        return null;
    }

    public virtual string GetData(string key, out bool isHandled)
    {
        isHandled = false;
        return "";
    }

    public virtual void SetData(string key, string value, out bool isHandled) => isHandled = false;
    public void DeleteData(string key, out bool isHandled) => isHandled = false;
    public bool IsKeySet(string key) => false;

    public virtual Dictionary<string, string> GetAllData(out bool isHandled)
    {
        isHandled = false;
        return null;
    }

    public virtual Dictionary<string, string> GetLeaderBoard(string key, out bool isHandled, bool orderFromHighest = true, ushort size = 0)
    {
        isHandled = false;
        return new Dictionary<string, string>();
    }

    public DatabaseAttribute Attribute { get; set; }
    
    public virtual void Load() { }
}