using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Player;

public class CustomInfoList :
    IList<CustomInfoList.CustomInfoEntry>,
    IReadOnlyList<CustomInfoList.CustomInfoEntry>,
    IJoinUpdate
{
    private readonly SynapsePlayer _player;
    private readonly PlayerService _playerService;
    private readonly MirrorService _mirrorService;
    private readonly List<CustomInfoEntry> _values = new();


    internal CustomInfoList(SynapsePlayer player, PlayerService playerService, MirrorService mirrorService,
        PlayerEvents playerEvent)
    {
        _player = player;
        _playerService = playerService;
        _mirrorService = mirrorService;
        playerEvent.SimpleSetClass.Subscribe(ev => UpdatePlayer(ev.Player));
    }

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
            UpdateInfo();
        }
    }

    public void Add(CustomInfoEntry item)
    {
        if (item == null) return;
        
        _values.Add(item);
        if (!item.EveryoneCanSee)
            IsForEveryoneEqual = false;

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
        UpdateInfo();

        return result;
    }
    
    public void RemoveAt(int index)
    {
        _values.RemoveAt(index);

        CheckAllEntries();
        UpdateInfo();
    }

    public void Clear()
    {
        _values.Clear();
        IsForEveryoneEqual = true;
        UpdateInfo();
    }

    public void Insert(int index, CustomInfoEntry item)
    {
        if (item == null) return;
        
        _values.Insert(index, item);
        if (!item.EveryoneCanSee)
            IsForEveryoneEqual = false;
        UpdateInfo();
    }
    
    

    public bool Contains(string item)
        => _values.Any(x => x.Info == item);

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
    public void UpdatePlayer(SynapsePlayer player)
    {
        var values = new List<string>();
        foreach (var entry in _values)
        {
            if (entry.EveryoneCanSee || entry.PlayersThatCanSee.Contains(player) || entry.SeeCondition(player))
            {
                values.Add(entry.Info);
            }
        }

        player.SendNetworkMessage(_mirrorService.GetCustomVarMessage(_player.NicknameSync, writer =>
        {
            writer.WriteUInt64(2ul);
            writer.WriteString(string.Join("\n", values));
        }));
    }
}