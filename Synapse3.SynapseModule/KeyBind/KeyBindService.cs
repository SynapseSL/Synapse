using Neuron.Core.Meta;
using Ninject;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.KeyBind.SynapseBind;
using Synapse3.SynapseModule.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Synapse3.SynapseModule.KeyBind;

public class KeyBindService : Service
{
    public const string DataBaseKey = "KeyBindSave";

    private readonly IKernel _kernel;
    private readonly Synapse _synapseModule;
    private readonly PlayerService _player;
    private readonly PlayerEvents _playerEvents;

    private Dictionary<string, IKeyBind> _binds = new();

    public KeyBindService(IKernel kernel, Synapse synapseModule, PlayerService player, PlayerEvents playerEvents)
    {
        _kernel = kernel;
        _synapseModule = synapseModule;
        _player = player;
    }
    
    public override void Enable()
    {
        _playerEvents.Join.Subscribe(LoadData);
        _playerEvents.Leave.Subscribe(SaveData);
        _playerEvents.KeyPress.Subscribe(KeyPress);

        RegisterKey<ScpSwitchChat>();

        while (_synapseModule.ModuleKeyBindBindingQueue.Count != 0)
        {
            var binding = _synapseModule.ModuleKeyBindBindingQueue.Dequeue();
            LoadBinding(binding);
        }
    }

    public override void Disable()
    {
        _playerEvents.Join.Unsubscribe(LoadData);
        _playerEvents.Leave.Unsubscribe(SaveData);
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

        var keyBind = (IKeyBind)_kernel.Get(keyBindType);
        _kernel.Bind(keyBindType).ToConstant(keyBind).InSingletonScope();

        keyBind.Attribute = info;
        keyBind.Load();

        _binds[info.CommandName.ToLower()] = keyBind;
        CheckForPlayers();
    }

    public void RegisterKey(IKeyBind keyBind, KeyBindAttribute info)
    {
        if (IsRegistered(info.CommandName)) return;

        keyBind.Attribute = info;

        _binds[info.CommandName.ToLower()] = keyBind;
        CheckForPlayers();
    }

    private void CheckForPlayers()
    {
        if (!_player.Players.Any()) return;

        foreach (var player in _player.Players)
        {
            CheckKey(player);
        }
    }


    public bool IsRegistered(string name)
        => _binds.ContainsKey(name.ToLower());

    public IKeyBind GetBind(string name)
        => _binds[name.ToLower()];

    private void LoadData(JoinEvent join)
    {
        var data = join.Player.GetData(DataBaseKey);

        if (!TryParseData(data, out var commandKey))
            commandKey = GenerateDefaultBind();

        join.Player._commandKey = commandKey;

        CheckKey(join.Player);
    }

    private void KeyPress(KeyPressEvent keyPress)
    {
        if (!keyPress.Player.CommandKey.TryGetValue(keyPress.KeyCode, out var commands))
            return;

        foreach (var command in commands)
        {
            command.Execute(keyPress.Player);
        }
    }

    private void SaveData(LeaveEvent leave)
    {
        string data = "";
        foreach (var keyBinds in leave.Player.CommandKey)
        {
            foreach (var bind in keyBinds.Value)
            {
                data += $"^^{keyBinds.Key}^^-^^{bind.Attribute.CommandName}^^";
            }
        }

        leave.Player.SetData(DataBaseKey, data);
    }

    //^^Command^^-^^Bind^^
    static Regex Regex = new Regex("(\\^\\^(.*?)\\^\\^-\\^\\^(.*?)\\^\\^)*");

    private bool TryParseData(string data, out Dictionary<KeyCode, List<IKeyBind>> commandKey)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            commandKey = null;
            return false;
        }

        var math = Regex.Match(data);
        if (!math.Success)
        {
            commandKey = null;
            return false;
        }

        commandKey = new Dictionary<KeyCode, List<IKeyBind>>();

        var length = math.Groups.Count;
        var commadName = "";
        for (int i = 0; i < length; i++)
        {
            var group = math.Groups[i];
            var value = group.Value;
            if (i % 2 == 0)
            {
                commadName = value;
            }
            else
            {
                var key = (KeyCode)Enum.Parse(typeof(KeyCode), value);
                var bind = GetBind(commadName);
                if (!commandKey.TryGetValue(key, out var commands))
                    commandKey[key] = commands = new List<IKeyBind>();

                commands.Add(bind);
            }
        }

        return true;
    }

    private Dictionary<KeyCode, List<IKeyBind>> GenerateDefaultBind()
    {
        var commandKey = new Dictionary<KeyCode, List<IKeyBind>>();

        foreach (var bind in _binds.Values)
        {
            var commadName = bind.Attribute.CommandName;
            var key = bind.Attribute.Bind;
            if (!commandKey.TryGetValue(key, out var commands))
                commandKey[key] = commands = new List<IKeyBind>();

            commands.Add(bind);
        }

        return commandKey;
    }

    private void CheckKey(SynapsePlayer player)
    {

        var playerBinds = player._commandKey.Values.SelectMany(p => p).ToList();
        var bindsToAdd = new List<IKeyBind>();

        foreach (var bind in _binds.Values)
        {
            if (playerBinds.Contains(bind))
                playerBinds.Remove(bind);
            else
                bindsToAdd.Add(bind);
        }

        foreach (var bind in bindsToAdd)
        {
            var key = bind.Attribute.Bind;
            if (!player._commandKey.TryGetValue(key, out var commands))
                player._commandKey[key] = commands = new List<IKeyBind>();

            commands.Add(bind);
        }

    }

}
