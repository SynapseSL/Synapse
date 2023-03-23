using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MEC;
using Synapse3.SynapseModule.Events;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public class CustomInfoList :
    IList<CustomInfoList.CustomInfoEntry>,
    IReadOnlyList<CustomInfoList.CustomInfoEntry>,
    IJoinUpdate
{
    private readonly SynapsePlayer _player;
    private readonly PlayerService _playerService;
    private readonly List<CustomInfoEntry> _values = new();

    internal CustomInfoList(SynapsePlayer player, PlayerService playerService,
        PlayerEvents playerEvent)
    {
        _player = player;
        _playerService = playerService; 
    }

    public bool AutomaticUpdateOnChange { get; set; } = true;

    public bool IsForEveryoneEqual { get; private set; } = true;

    public void CheckAllEntries()
    {
        IsForEveryoneEqual = true;
        foreach (var entry in _values)
        {
            if (entry.EveryoneCanSee) continue;
            
            IsForEveryoneEqual = false;
            return;
        }
    }

    public void UpdateInfo()
    {
        if (IsForEveryoneEqual)
        {
            _player.NicknameSync.Network_customPlayerInfoString =
                _values.Count <= 0 ? string.Empty : string.Join("\n", _values.Select(x => x.Info));
            return;
        }

        foreach (var player in _playerService.Players)
        {
            UpdatePlayer(player);
        }
    }
    
    public class CustomInfoEntry
    {
        public string Info { get; set; }

        public bool EveryoneCanSee { get; set; } = true;

        public List<SynapsePlayer> PlayersThatCanSee { get; set; } = new();

        public Func<SynapsePlayer, bool> SeeCondition { get; set; } = _ => true;
    }

    public CustomInfoEntry this[int index]
    {
        get => _values[index];
        set
        {
            _values[index] = value;
            CheckAllEntries();
            if (AutomaticUpdateOnChange)
                UpdateInfo();
        }
    }

    public void Add(CustomInfoEntry item)
    {
        if (item == null) return;
        
        _values.Add(item);
        if (!item.EveryoneCanSee)
            IsForEveryoneEqual = false;
        
        if (AutomaticUpdateOnChange)
            UpdateInfo();
    }

    public void Add(string item)
        => Add(new CustomInfoEntry()
        {
            Info = item
        });
    
    public void Add(string item, List<SynapsePlayer> players)
        => Add(new CustomInfoEntry()
        {
            EveryoneCanSee = false,
            Info = item,
            PlayersThatCanSee = players
        });

    public void Add(string item, Func<SynapsePlayer, bool> func)
        => Add(new CustomInfoEntry()
        {
            EveryoneCanSee = false,
            Info = item,
            SeeCondition = func
        });
    
    public bool Remove(CustomInfoEntry item)
    {
        var result = _values.Remove(item);

        if(item?.EveryoneCanSee != false)
            CheckAllEntries();
        
        if (AutomaticUpdateOnChange)
            UpdateInfo();

        return result;
    }
    
    public void RemoveAt(int index)
    {
        _values.RemoveAt(index);

        CheckAllEntries();
        if (AutomaticUpdateOnChange)
            UpdateInfo();
    }

    public void Clear()
    {
        _values.Clear();
        IsForEveryoneEqual = true;
        if (AutomaticUpdateOnChange)
            UpdateInfo();
    }

    public void Insert(int index, CustomInfoEntry item)
    {
        if (item == null) return;
        
        _values.Insert(index, item);
        if (!item.EveryoneCanSee)
            IsForEveryoneEqual = false;
        
        if (AutomaticUpdateOnChange)
            UpdateInfo();
    }
    

    public bool Contains(CustomInfoEntry item)
        => _values.Contains(item);

    public void CopyTo(CustomInfoEntry[] array, int arrayIndex)
        => _values.CopyTo(array, arrayIndex);
    
    public int IndexOf(CustomInfoEntry item)
        => _values.IndexOf(item);
    
    
    
    public IEnumerator<CustomInfoEntry> GetEnumerator() => _values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

    int ICollection<CustomInfoEntry>.Count => _values.Count;

    public bool IsReadOnly => false;

    int IReadOnlyCollection<CustomInfoEntry>.Count => _values.Count;
    public bool NeedsJoinUpdate => true;

    private readonly Dictionary<SynapsePlayer, float> _lastUpdates = new();
    private readonly Dictionary<SynapsePlayer, bool> _activeUpdate = new();

    public void UpdatePlayer(SynapsePlayer player)
    {
        if (_activeUpdate.ContainsKey(player) && _activeUpdate[player]) return;
        if (_lastUpdates.ContainsKey(player) && Time.time < _lastUpdates[player] + 0.1f)
        {
            _activeUpdate[player] = true;
            Timing.CallDelayed(0.1f, () =>
            {
                _activeUpdate[player] = false;
                UpdatePlayer(player);
            });
            return;
        }
        
        _lastUpdates[player] = Time.time;
        var values = new List<string>();
        foreach (var entry in _values)
        {
            if (entry.EveryoneCanSee || entry.PlayersThatCanSee.Contains(player) || entry.SeeCondition(player))
            {
                values.Add(entry.Info);
            }
        }
        
        player.SendFakeSyncVar(_player.NicknameSync, 2ul, string.Join("\n", values));
    }
}