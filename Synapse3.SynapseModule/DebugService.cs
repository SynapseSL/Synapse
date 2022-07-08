using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map;
using UnityEngine;

namespace Synapse3.SynapseModule;

#if DEBUG
public class DebugService : Service
{
    private PlayerEvents _player;

    public DebugService(PlayerEvents player)
    {
        _player = player;
    }

    public override void Enable()
    {
        _player.KeyPress.Subscribe(OnKeyPress);
    }

    public override void Disable()
    {
        _player.KeyPress.Unsubscribe(OnKeyPress);
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