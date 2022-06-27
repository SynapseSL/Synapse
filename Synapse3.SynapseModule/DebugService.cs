using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;
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

    private void OnKeyPress(KeyPressEvent ev)
    {
        switch (ev.KeyCode)
        {
            case KeyCode.Alpha1:
                NeuronLogger.For<Synapse>().Warn($"All Items: {ev.Player.Inventory.Items.Count}");
                break;
        }
    }
}
#endif