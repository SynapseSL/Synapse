using Neuron.Core.Meta;
using Ninject;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.KeyBind.SynapseBind;
using Synapse3.SynapseModule.Player;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Neuron.Core.Dev;
using Synapse3.SynapseModule.Config;
using UnityEngine;

namespace Synapse3.SynapseModule.KeyBind;

public class KeyBindService : Service
{
    public const string DataBaseKey = "KeyBindSave";
    
    private readonly Synapse _synapseModule;
    private readonly PlayerService _player;
    private readonly PlayerEvents _playerEvents;
    private readonly SynapseConfigService _config;

    private readonly List<IKeyBind> _binds = new();
    internal Dictionary<SynapsePlayer, IKeyBind> NewBinding = new();
    public ReadOnlyCollection<IKeyBind> DefaultBinds => _binds.AsReadOnly();

    public KeyBindService(Synapse synapseModule, PlayerService player, PlayerEvents playerEvents,
        SynapseConfigService config)
    {
        _synapseModule = synapseModule;
        _player = player;
        _playerEvents = playerEvents;
        _config = config;
    }
    
    public override void Enable()
    {
        _playerEvents.Join.Subscribe(LoadData);
        _playerEvents.KeyPress.Subscribe(KeyPress);

        RegisterKey<ScpSwitchChat>();
        #if DEV
        RegisterKey<Dev>();
        #endif

        while (_synapseModule.ModuleKeyBindBindingQueue.Count != 0)
        {
            var binding = _synapseModule.ModuleKeyBindBindingQueue.Dequeue();
            LoadBinding(binding);
        }
    }

    public override void Disable()
    {
        _playerEvents.Join.Unsubscribe(LoadData);
        _playerEvents.KeyPress.Unsubscribe(KeyPress);
    }

    internal void LoadBinding(SynapseKeyBindBinding binding) => RegisterKey(binding.Type, binding.Info);

    public void RegisterKey<TKeyBind>() where TKeyBind : IKeyBind
    {
        var info = typeof(TKeyBind).GetCustomAttribute<KeyBindAttribute>();
        if (info == null) return;

        RegisterKey(typeof(TKeyBind), info);
    }

    public void RegisterKey(Type keyBindType, KeyBindAttribute info)
    {
        if (IsRegistered(info.CommandName)) return;
        if (!typeof(IKeyBind).IsAssignableFrom(keyBindType)) return;

        var keyBind = (IKeyBind)Synapse.GetOrCreate(keyBindType);
        keyBind.Attribute = info;
        keyBind.Load();

        _binds.Add(keyBind);
        CheckForPlayers();
    }

    public void RegisterKey(IKeyBind keyBind, KeyBindAttribute info)
    {
        if (IsRegistered(info.CommandName)) return;

        keyBind.Attribute = info;
        keyBind.Load();

        _binds.Add(keyBind);
        CheckForPlayers();
    }


    public bool IsRegistered(string name)
        => _binds.Any(x => string.Equals(name, x.Attribute?.CommandName, StringComparison.OrdinalIgnoreCase));

    public IKeyBind GetBind(string name)
        => _binds.FirstOrDefault(x =>
            string.Equals(name, x.Attribute?.CommandName, StringComparison.OrdinalIgnoreCase));

    private void LoadData(JoinEvent ev)
    {
        if (ev.Player.DoNotTrack)
        {
            return;
        }
        
        var data = ev.Player.GetData(DataBaseKey);

        if (!TryParseData(data, out var commandKey))
            commandKey = GenerateDefaultBind();

        ev.Player._binds = commandKey;

        CheckKey(ev.Player);
    }

    //TODO: Improve this code when I have more time to do so
    private void KeyPress(KeyPressEvent ev)
    {
        if (NewBinding.ContainsKey(ev.Player))
        {
            var newBind = NewBinding[ev.Player];
            
            foreach (var bindPair in ev.Player.Binds)
            {
                foreach (var bind in bindPair.Value.ToList())
                {
                    if (bind == newBind)
                        ev.Player._binds[bindPair.Key].Remove(bind);
                }
            }

            if (!ev.Player._binds.TryGetValue(ev.KeyCode, out var binds))
                ev.Player._binds[ev.KeyCode] = binds = new List<IKeyBind>();

            binds.Add(newBind);
            ev.Player.SendConsoleMessage(
                _config.Translation.Get(ev.Player).KeyBindSet
                    .Format(newBind.Attribute.CommandName, ev.KeyCode), "green");
            NewBinding.Remove(ev.Player);
            
            //Store the info inside the Database
            if(ev.Player.DoNotTrack) return;
            if (!TryParseData(ev.Player.GetData(DataBaseKey), out var storedBinds))
                storedBinds = new Dictionary<KeyCode, List<IKeyBind>>();

            foreach (var bindPair in storedBinds)
            {
                foreach (var bind in bindPair.Value.ToList())
                {
                    if (bind == newBind)
                        storedBinds[bindPair.Key].Remove(bind);
                }   
            }

            if (!storedBinds.ContainsKey(ev.KeyCode))
                storedBinds[ev.KeyCode] = new List<IKeyBind>();
            
            storedBinds[ev.KeyCode].Add(newBind);
                    
                    
            var data = "";
            foreach (var keyBinds in storedBinds)
            {
                if (keyBinds.Value.Count == 0) continue;
                data += keyBinds.Key + ";";
                for (int i = 0; i < keyBinds.Value.Count; i++)
                {
                    var bind = keyBinds.Value[i];
                    if (i > 0)
                        data += "-";
                    data += bind.Attribute.CommandName;
                }
                data += "/";
            }
            ev.Player.SetData(DataBaseKey, data);
            return;
        }
        
        if (!ev.Player.Binds.TryGetValue(ev.KeyCode, out var commands))
            return;

        foreach (var command in commands)
        {
            command.Execute(ev.Player);
        }
    }

    private bool TryParseData(string data, out Dictionary<KeyCode, List<IKeyBind>> commandKey)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            commandKey = null;
            return false;
        }

        var dic = new Dictionary<KeyCode, List<IKeyBind>>();
        var keyCodes = data.Split('/');
        foreach (var keyString in keyCodes)
        {
            var info = keyString.Split(';');
            if (info.Length < 2) continue;
            if (!Enum.TryParse(info[0], out KeyCode code)) continue;
            
            var binds = new List<IKeyBind>();
            foreach (var bindingName in info[1].Split('-'))
            {
                var bind = GetBind(bindingName);
                if (bind == null) continue;
                binds.Add(bind);
            }
            
            dic[code] = binds;
        }

        if (dic.Count == 0)
        {
            commandKey = null;
            return false;
        }

        commandKey = dic;
        return true;
    }

    private Dictionary<KeyCode, List<IKeyBind>> GenerateDefaultBind()
    {
        var commandKey = new Dictionary<KeyCode, List<IKeyBind>>();

        foreach (var bind in _binds)
        {
            var key = bind.Attribute.Bind;
            if (!commandKey.TryGetValue(key, out var commands))
                commandKey[key] = commands = new List<IKeyBind>();

            commands.Add(bind);
        }

        return commandKey;
    }

    private void CheckForPlayers()
    {
        if (!_player.Players.Any()) return;

        foreach (var player in _player.Players)
        {
            CheckKey(player);
        }
    }
    
    private void CheckKey(SynapsePlayer player)
    {
        var playerBinds = player._binds.Values.SelectMany(p => p).ToList();
        foreach (var bind in _binds)
        {
            if (!playerBinds.Contains(bind))
                AddDefaultBindToPlayer(player, bind);
        }
    }

    private void AddDefaultBindToPlayer(SynapsePlayer player, IKeyBind bind)
    {
        var key = bind.Attribute.Bind;
        if (!player._binds.TryGetValue(key, out var commands))
            player._binds[key] = commands = new List<IKeyBind>();

        commands.Add(bind);
    }
}
