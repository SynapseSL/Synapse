using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;
using UnityEngine;

namespace Synapse3.SynapseModule;

#if DEBUG
public class DebugService : Service
{
    private PlayerEvents _player;
    private MapEvents _map;

    public DebugService(PlayerEvents player, MapEvents map)
    {
        _player = player;
        _map = map;
    }

    public override void Enable()
    {
        _map.Scp914Upgrade.Subscribe(Upgrade);
        _player.KeyPress.Subscribe(OnKeyPress);
    }

    public override void Disable()
    {
        _map.Scp914Upgrade.Unsubscribe(Upgrade);
        _player.KeyPress.Unsubscribe(OnKeyPress);
    }

    private void Upgrade(Scp914UpgradeEvent ev)
    {
        ev.MoveItems = false;
        ev.MovePlayers = false;
    }

    private void OnKeyPress(KeyPressEvent ev)
    {
        switch (ev.KeyCode)
        {
            case KeyCode.Alpha1:
                var comp = ev.Player.GetComponentInChildren<HitboxIdentity>();
                NeuronLogger.For<Synapse>().Warn(comp == null);
                break;
        }
    }
}
#endif