using System;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Neuron.Modules.Commands.Event;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.CustomRole;

public class CustomRoleService : Service
{
    private readonly SynapseCommandService _command;
    private readonly PlayerService _player;

    public CustomRoleService(SynapseCommandService command, PlayerService player)
    {
        _command = command;
        _player = player;
    }

    public override void Enable()
    {
        _command.RemoteAdmin.Subscribe(OnRemoteAdmin);
    }

    /// <summary>
    /// This is just to remove the Custom Role when someone is forced to another role
    /// </summary>
    /// <param name="ev"></param>
    private void OnRemoteAdmin(CommandEvent ev)
    {
        if(!string.Equals(ev.Context.Command,"overwatch",StringComparison.OrdinalIgnoreCase) &&
           !string.Equals(ev.Context.Command,"kill",StringComparison.OrdinalIgnoreCase) &&
           !string.Equals(ev.Context.Command,"forceclass",StringComparison.OrdinalIgnoreCase)) return;
        
        if(ev.Context.Arguments.Length == 0) return;

        var ids = ev.Context.Arguments[0].Split('.');
        foreach (var id in ids)
        {
            if (string.IsNullOrEmpty(id))
                continue;
            
            if(!int.TryParse(id,out var result)) continue;

            var player = _player.GetPlayer(result);
            if (player == null) continue;

            if (player.CustomRole != null)
                player.RemoveCustomRole(DespawnReason.Forceclass);
        }
    }
}